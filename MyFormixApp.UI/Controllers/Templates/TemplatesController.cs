using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.DTOs.Templates;
using MyFormixApp.Domain.DTOs.Questions;
using FluentValidation;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;

namespace MyFormixApp.UI.Controllers
{
    [Authorize]
    public class TemplatesController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly ITemplateThemeService _themeService;
        private readonly IValidator<TemplateDto> _validator;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly IMapper _mapper;

        public TemplatesController(
            ITemplateService templateService,
            ITemplateThemeService themeService,
            IValidator<TemplateDto> validator,
            ICloudStorageService cloudStorageService,
            IMapper mapper)
        {
            _templateService = templateService;
            _themeService = themeService;
            _validator = validator;
            _cloudStorageService = cloudStorageService;
            _mapper = mapper;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("User ID not found"));

        public async Task<IActionResult> Index() =>
            View(await GetTemplatesForUser());

        public async Task<IActionResult> Create()
        {
            await SetThemesInViewBag();
            return View(CreateDefaultTemplateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TemplateDto dto, IFormFile image)
        {
            await ProcessImage(dto, image);
            await ParseFormValues(dto);
            return await TryCreate(dto);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var template = await GetTemplateForEdit(id);
            if (template == null) return NotFound();
            await SetThemesInViewBag();
            return View(_mapper.Map<TemplateDto>(template));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TemplateDto dto, IFormFile image)
        {
            if (id != dto.Id) return NotFound();
            await ReplaceImageIfNeeded(dto, image);
            var result = await TryUpdate(dto);
            return result ?? NotFound();
        }

        public async Task<IActionResult> Delete(Guid id) =>
            await CanEdit(id)
                ? View(await _templateService.GetByIdAsync(id, CurrentUserId))
                : NotFound();

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id) =>
            await _templateService.DeleteAsync(id, IsAdmin() ? null : CurrentUserId)
                ? RedirectToAction(nameof(Index))
                : NotFound();

        private bool IsAdmin() => User.IsInRole("Admin");

        private async Task<IEnumerable<TemplateListItemDto>> GetTemplatesForUser() =>
            IsAdmin()
                ? await _templateService.GetAllTemplatesAsync()
                : await _templateService.GetUserTemplatesAsync(CurrentUserId);


        private TemplateDto CreateDefaultTemplateDto() =>
            new() { Tags = new(), ThemeId = 0, IsPublic = true };

        private async Task SetThemesInViewBag() =>
            ViewBag.Themes = await _themeService.GetAllThemesAsync();

        private async Task ProcessImage(TemplateDto dto, IFormFile image)
        {
            if (image?.Length > 0)
                dto.ImageUrl = await _cloudStorageService.UploadFileAsync(image, "templates");
        }

        private async Task ReplaceImageIfNeeded(TemplateDto dto, IFormFile image)
        {
            if (image?.Length > 0)
            {
                if (!string.IsNullOrEmpty(dto.ImageUrl))
                    await _cloudStorageService.DeleteFileAsync(dto.ImageUrl, "templates");

                dto.ImageUrl = await _cloudStorageService.UploadFileAsync(image, "templates");
            }
        }

        private async Task ParseFormValues(TemplateDto dto)
        {
            dto.Tags = ParseTagsFromForm();
            dto.Questions = ParseQuestionsFromForm();
        }

        private async Task<IActionResult> TryCreate(TemplateDto dto)
        {
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid) return await ValidationFailed(dto, result);

            try
            {
                var created = await _templateService.CreateAsync(dto, CurrentUserId);
                return RedirectToAction("Details", "TemplateViews", new { id = created.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating template: " + ex.Message);
                await SetThemesInViewBag();
                return View(dto);
            }
        }

        private async Task<IActionResult?> TryUpdate(TemplateDto dto)
        {
            var template = await _templateService.GetByIdAsync(dto.Id, CurrentUserId);
            if (template == null || (!IsAdmin() && template.Author.Id != CurrentUserId)) return null;

            await ParseFormValues(dto);
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid) return await ValidationFailed(dto, result);

            try
            {
                var updated = await _templateService.UpdateAsync(dto, IsAdmin() ? null : CurrentUserId);
                return updated == null
                    ? null
                    : RedirectToAction("Details", "TemplateViews", new { id = dto.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating template: " + ex.Message);
                await SetThemesInViewBag();
                return View(dto);
            }
        }

        private async Task<IActionResult> ValidationFailed(TemplateDto dto, FluentValidation.Results.ValidationResult result)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(e.PropertyName, e.ErrorMessage);

            await SetThemesInViewBag();
            return View(dto);
        }

        private async Task<TemplateDto?> GetTemplateForEdit(Guid id)
        {
            var template = await _templateService.GetByIdAsync(id, CurrentUserId);
            return template == null || (!IsAdmin() && template.Author.Id != CurrentUserId)
                ? null
                : template;
        }

        private async Task<bool> CanEdit(Guid id)
        {
            var template = await _templateService.GetByIdAsync(id, CurrentUserId);
            return template != null && (IsAdmin() || template.Author.Id == CurrentUserId);
        }

        private List<string> ParseTagsFromForm() =>
            Request.Form.ContainsKey("Tags")
                ? Request.Form["Tags"].ToString()
                    .Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList()
                : new();

        private List<QuestionDto> ParseQuestionsFromForm()
        {
            if (!Request.Form.ContainsKey("QuestionsData")) return new();
            var json = Request.Form["QuestionsData"];
            return string.IsNullOrEmpty(json)
                ? new()
                : JsonSerializer.Deserialize<List<QuestionDto>>(json) ?? new();
        }
    }
}
