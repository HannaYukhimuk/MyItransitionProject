using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.DTOs.Templates;
using FluentValidation;

namespace MyFormixApp.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TemplateThemesController : Controller
    {
        private readonly ITemplateThemeService _themeService;
        private readonly IValidator<TemplateThemeDto> _validator;

        public TemplateThemesController(
            ITemplateThemeService themeService,
            IValidator<TemplateThemeDto> validator)
        {
            _themeService = themeService;
            _validator = validator;
        }

        public async Task<IActionResult> Index() =>
            View(await _themeService.GetAllThemesAsync());

        public async Task<IActionResult> Details(int id) =>
            await LoadThemeOrNotFound(id, theme => View(theme));

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TemplateThemeDto dto)
        {
            if (!await ValidateDtoAsync(dto)) return View(dto);

            var created = await _themeService.CreateThemeAsync(dto);
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        public async Task<IActionResult> Edit(int id) =>
            await LoadThemeOrNotFound(id, theme => View(theme));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TemplateThemeDto dto)
        {
            if (id != dto.Id) return BadRequest();
            if (!await ValidateDtoAsync(dto)) return View(dto);

            var updated = await _themeService.UpdateThemeAsync(id, dto);
            return updated == null ? NotFound() : RedirectToAction(nameof(Details), new { id = updated.Id });
        }

        public async Task<IActionResult> Delete(int id) =>
            await LoadThemeOrNotFound(id, theme => View(theme));

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            return await _themeService.DeleteThemeAsync(id)
                ? RedirectToAction(nameof(Index))
                : NotFound();
        }

        // Helpers
        private async Task<bool> ValidateDtoAsync(TemplateThemeDto dto)
        {
            var result = await _validator.ValidateAsync(dto);
            if (result.IsValid) return true;

            foreach (var error in result.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return false;
        }

        private async Task<IActionResult> LoadThemeOrNotFound(int id, Func<TemplateThemeDto, IActionResult> onFound)
        {
            var theme = await _themeService.GetThemeByIdAsync(id);
            return theme == null ? NotFound() : onFound(theme);
        }
    }
}
