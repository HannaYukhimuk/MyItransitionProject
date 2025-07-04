using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.DTOs.Templates;
using MyFormixApp.Domain.DTOs.Questions;
using FluentValidation;
using System.Security.Claims;
using System.Text.Json;

namespace MyFormixApp.UI.Controllers
{
    [Authorize]
    public class TemplatesController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly ITemplateThemeService _themeService;
        private readonly IValidator<TemplateDto> _validator;
        private readonly ICloudStorageService _cloudStorageService;

        public TemplatesController(
            ITemplateService templateService,
            ITemplateThemeService themeService,
            IValidator<TemplateDto> validator,
            ICloudStorageService cloudStorageService)
        {
            _templateService = templateService;
            _themeService = themeService;
            _validator = validator;
            _cloudStorageService = cloudStorageService;
        }

        private Guid CurrentUserId => 
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
            throw new InvalidOperationException("User ID not found"));

        public async Task<IActionResult> Index() => 
            View(await GetTemplatesForCurrentUser());

        public async Task<IActionResult> Create()
        {
            ViewBag.Themes = await _themeService.GetAllThemesAsync();
            return View(await CreateDefaultTemplateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TemplateDto dto, IFormFile templateImage)
        {
            await ProcessImageUpload(dto, templateImage);
            if (!await ValidateAndProcessTemplate(dto, "create")) return View(dto);
            return RedirectToCreatedTemplate(await _templateService.CreateAsync(dto, CurrentUserId));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var template = await GetTemplateForEdit(id);
            if (template == null) return NotFound();
            return View(await MapToTemplateDto(template));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TemplateDto dto, IFormFile templateImage)
        {
            if (id != dto.Id) return NotFound();
            await ProcessImageUpload(dto, templateImage);
            if (!await ValidateAndProcessTemplate(dto, "edit", id)) return View(dto);
            return RedirectToUpdatedTemplate(id);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var template = await GetTemplateForDelete(id);
            return template == null ? NotFound() : View(template);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id) => 
            await _templateService.DeleteAsync(id, GetDeleteUserId()) ? 
            RedirectToAction(nameof(Index)) : 
            NotFound();

        private async Task<IEnumerable<TemplateListItemDto>> GetTemplatesForCurrentUser() => 
            User.IsInRole("Admin") ? 
            await _templateService.GetAllTemplatesAsync() : 
            await _templateService.GetUserTemplatesAsync(CurrentUserId);

        private async Task<TemplateDto> CreateDefaultTemplateDto() => new()
        {
            Tags = new(),
            ThemeId = (await _themeService.GetAllThemesAsync()).FirstOrDefault()?.Id ?? 0,
            IsPublic = true
        };

        private async Task ProcessImageUpload(TemplateDto dto, IFormFile image)
        {
            if (image?.Length <= 0) return;
            dto.ImageUrl = await _cloudStorageService.UploadFileAsync(image, "templates");
        }

        private async Task<bool> ValidateAndProcessTemplate(TemplateDto dto, string action, Guid? id = null)
        {
            SetTemplateDataFromForm(dto);
            var validation = await _validator.ValidateAsync(dto);
            if (validation.IsValid) return true;
            AddValidationErrors(validation);
            await LoadThemesForView();
            return false;
        }

        private void SetTemplateDataFromForm(TemplateDto dto)
        {
            dto.Tags = ParseTagsFromForm();
            dto.Questions = ParseQuestionsFromForm();
        }

        private void AddValidationErrors(FluentValidation.Results.ValidationResult validation)
        {
            foreach (var e in validation.Errors)
                ModelState.AddModelError(e.PropertyName, e.ErrorMessage);
        }

        private async Task LoadThemesForView() => 
            ViewBag.Themes = await _themeService.GetAllThemesAsync();

        private IActionResult RedirectToCreatedTemplate(TemplateDetailsDto created) => 
            RedirectToAction("Details", "TemplateViews", new { id = created.Id });

        private IActionResult RedirectToUpdatedTemplate(Guid id) => 
            RedirectToAction("Details", "TemplateViews", new { id });

        private async Task<TemplateDetailsDto?> GetTemplateForEdit(Guid id)
        {
            var template = await _templateService.GetByIdAsync(id, CurrentUserId);
            return template == null || !CanEditTemplate(template) ? null : template;
        }

        private bool CanEditTemplate(TemplateDetailsDto template) => 
            User.IsInRole("Admin") || template.Author.Id == CurrentUserId;

        private async Task<TemplateDto> MapToTemplateDto(TemplateDetailsDto template)
        {
            var themes = await _themeService.GetAllThemesAsync();
            return new TemplateDto
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                ThemeId = template.Theme.Id,
                IsPublic = template.IsPublic,
                Tags = template.Tags,
                AllowedUserIds = template.AllowedUserIds,
                Questions = template.Questions.Select(q => new QuestionDto
                {
                    Type = q.Type,
                    Title = q.Title,
                    Description = q.Description,
                    IsRequired = q.IsRequired,
                    Position = q.Position,
                    Options = q.Options?.Select(o => new QuestionOptionDto
                    {
                        Text = o.Text,
                        Position = o.Position
                    }).ToList() ?? new()
                }).ToList()
            };
        }

        private async Task<TemplateDetailsDto?> GetTemplateForDelete(Guid id)
        {
            var template = await _templateService.GetByIdAsync(id, CurrentUserId);
            return template == null || !CanEditTemplate(template) ? null : template;
        }

        private Guid? GetDeleteUserId() => 
            User.IsInRole("Admin") ? null : CurrentUserId;

        private List<string> ParseTagsFromForm() => 
            Request.Form.ContainsKey("Tags") ? 
            Request.Form["Tags"].ToString()
                .Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList() : 
            new();

        private List<QuestionDto> ParseQuestionsFromForm() => 
            Request.Form.ContainsKey("QuestionsData") && 
            !string.IsNullOrEmpty(Request.Form["QuestionsData"]) ? 
            JsonSerializer.Deserialize<List<QuestionDto>>(Request.Form["QuestionsData"]) ?? new() : 
            new();
    }
}