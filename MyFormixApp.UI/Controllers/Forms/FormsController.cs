using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.DTOs.Forms;
using FluentValidation;
using System.Security.Claims;

namespace MyFormixApp.UI.Controllers.Forms
{
    [Authorize]
    public class FormsController : Controller
    {
        private readonly IFormService _formService;
        private readonly IValidator<FormDto> _validator;

        public FormsController(IFormService formService, IValidator<FormDto> validator)
        {
            _formService = formService;
            _validator = validator;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        private IActionResult RedirectWithMessage(string action, string key, string message)
        {
            TempData[key] = message;
            return RedirectToAction(action);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var form = await _formService.GetByIdAsync(id, CurrentUserId);
            return form == null ? NotFound() : View(form);
        }

        [HttpGet]
        public async Task<IActionResult> My() =>
            View(await _formService.GetByUserAsync(CurrentUserId));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _formService.DeleteAsync(id, CurrentUserId);
            return result
                ? RedirectWithMessage("My", "Success", "Форма удалена.")
                : RedirectWithMessage("My", "Error", "Не удалось удалить форму.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] FormDto dto)
        {
            if (dto?.Id == null)
                return RedirectWithMessage(User.IsInRole("Admin") ? "Admin" : "My", "Error", "Invalid form submission.");

            try
            {
                var isAdmin = User.IsInRole("Admin");
                var success = await _formService.UpdateAsync(dto, CurrentUserId, isAdmin);

                if (!success)
                    return RedirectWithMessage("Details", "Error", "Update failed. You don't have permission to edit this form.");

                TempData["Success"] = "Form updated successfully.";
                return RedirectToAction(isAdmin ? "AdminEdit" : "Details", new { id = dto.Id });
            }
            catch (Exception ex)
            {
                return RedirectWithMessage("Details", "Error", ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AdminEdit(Guid id)
        {
            var form = await _formService.GetByIdAsync(id, CurrentUserId);
            if (form == null) return NotFound();

            ViewBag.IsAdminEdit = true;
            return View("Details", form);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Admin() =>
            View(await _formService.GetAllFormsAsync());

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminDelete(Guid id)
        {
            var result = await _formService.DeleteAsync(id, CurrentUserId);
            return result
                ? RedirectWithMessage("Admin", "Success", "Form deleted successfully.")
                : RedirectWithMessage("Admin", "Error", "Failed to delete form.");
        }
    }
}
