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
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new InvalidOperationException("User ID not found"));

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");
            var templates = isAdmin
                ? await _templateService.GetAllTemplatesAsync()
                : await _templateService.GetUserTemplatesAsync(CurrentUserId);

            return View(templates);
        }

        public async Task<IActionResult> Create()
        {
            var themes = await _themeService.GetAllThemesAsync();
            ViewBag.Themes = themes;

            return View(new TemplateDto
            {
                Tags = new(),
                ThemeId = themes.FirstOrDefault()?.Id ?? 0,
                IsPublic = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TemplateDto dto, IFormFile templateImage)
        {
            if (templateImage?.Length > 0)
                dto.ImageUrl = await _cloudStorageService.UploadFileAsync(templateImage, "templates");

            dto.Tags = ParseTagsFromForm();
            dto.Questions = ParseQuestionsFromForm();

            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                foreach (var e in validation.Errors)
                    ModelState.AddModelError(e.PropertyName, e.ErrorMessage);

                ViewBag.Themes = await _themeService.GetAllThemesAsync();
                return View(dto);
            }

            try
            {
                var created = await _templateService.CreateAsync(dto, CurrentUserId);
                return RedirectToAction("Details", "TemplateViews", new { id = created.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating template: " + ex.Message);
                ViewBag.Themes = await _themeService.GetAllThemesAsync();
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var template = await _templateService.GetByIdAsync(id, CurrentUserId);
            if (template == null || (!User.IsInRole("Admin") && template.Author.Id != CurrentUserId))
                return NotFound();

            ViewBag.Themes = await _themeService.GetAllThemesAsync();

            return View(new TemplateDto
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
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TemplateDto dto, IFormFile templateImage)
        {
            if (id != dto.Id) return NotFound();

            if (templateImage?.Length > 0)
            {
                if (!string.IsNullOrEmpty(dto.ImageUrl))
                    await _cloudStorageService.DeleteFileAsync(dto.ImageUrl, "templates");

                dto.ImageUrl = await _cloudStorageService.UploadFileAsync(templateImage, "templates");
            }

            var existing = await _templateService.GetByIdAsync(id, CurrentUserId);
            if (existing == null || (!User.IsInRole("Admin") && existing.Author.Id != CurrentUserId))
                return NotFound();

            dto.Tags = ParseTagsFromForm();
            dto.Questions = ParseQuestionsFromForm();

            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                foreach (var e in validation.Errors)
                    ModelState.AddModelError(e.PropertyName, e.ErrorMessage);

                ViewBag.Themes = await _themeService.GetAllThemesAsync();
                return View(dto);
            }

            try
            {
                var updated = await _templateService.UpdateAsync(dto, User.IsInRole("Admin") ? null : CurrentUserId);
                if (updated == null) return NotFound();
                return RedirectToAction("Details", "TemplateViews", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating template: " + ex.Message);
                ViewBag.Themes = await _themeService.GetAllThemesAsync();
                return View(dto);
            }
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var template = await _templateService.GetByIdAsync(id, CurrentUserId);
            if (template == null || (!User.IsInRole("Admin") && template.Author.Id != CurrentUserId))
                return NotFound();

            return View(template);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _templateService.DeleteAsync(id, User.IsInRole("Admin") ? null : CurrentUserId);
            return result ? RedirectToAction(nameof(Index)) : NotFound();
        }

        // Helpers
        private List<string> ParseTagsFromForm()
        {
            if (!Request.Form.ContainsKey("Tags")) return new();
            return Request.Form["Tags"].ToString()
                .Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
        }

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
