// FormsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.DTOs.Forms;
using System.Security.Claims;

namespace MyFormixApp.UI.Controllers.Forms
{
    [Authorize]
    public class FormsController : Controller
    {
        private readonly IFormService _formService;
        private readonly Guid _currentUserId;

        public FormsController(IFormService formService, IHttpContextAccessor httpContextAccessor)
        {
            _formService = formService;
            _currentUserId = Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), 
                out var id) ? id : Guid.Empty;
        }

        [HttpGet] 
public async Task<IActionResult> Details(Guid id)
{
    var result = await _formService.GetFormDetailsAsync(id, _currentUserId);
    
    return View(result.Data);
}

        [HttpGet] public async Task<IActionResult> My() => 
            View(await _formService.GetUserFormsAsync(_currentUserId));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _formService.DeleteFormAsync(id, _currentUserId);
            return RedirectToAction("My", new { message = result.Message, isSuccess = result.IsSuccess });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] FormDto dto)
        {
            var result = await _formService.UpdateFormAsync(dto, _currentUserId, User.IsInRole("Admin"));
            return RedirectToAction(result.RedirectAction, new { id = dto.Id, message = result.Message, isSuccess = result.IsSuccess });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet] 
        public async Task<IActionResult> AdminEdit(Guid id)
        {
            var result = await _formService.GetFormDetailsAsync(id, _currentUserId);
            return View("Details", result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet] public async Task<IActionResult> Admin() => 
            View(await _formService.GetAllFormsAsync());

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminDelete(Guid id)
        {
            var result = await _formService.DeleteFormAsync(id, _currentUserId);
            return RedirectToAction("Admin", new { message = result.Message, isSuccess = result.IsSuccess });
        }
    }
}