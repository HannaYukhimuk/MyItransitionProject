// FormResponsesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using System.Security.Claims;

namespace MyFormixApp.UI.Controllers.Forms
{
    [Authorize]
    public class FormResponsesController : Controller
    {
        private readonly IFormService _formService;
        private readonly Guid _currentUserId;

        public FormResponsesController(IFormService formService, IHttpContextAccessor httpContextAccessor)
        {
            _formService = formService;
            _currentUserId = Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), 
                out var id) ? id : Guid.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Take(Guid id)
        {
            var result = await _formService.ProcessFormResponseAsync(id, _currentUserId, Request.Form);
            return RedirectToAction("View", "TemplateViews", new { id, message = result.Message, isSuccess = result.IsSuccess });
        }

        [HttpGet("templates/{templateId}/forms")]
        public async Task<IActionResult> TemplateForms(Guid templateId)
        {
            var result = await _formService.GetTemplateFormsAsync(templateId, _currentUserId);
            if (!result.Success)
            {
                return RedirectToAction("Details", "Templates", new { id = templateId });
            }
            
            ViewBag.TemplateId = templateId;
            ViewBag.TemplateTitle = result.Data.TemplateTitle;
            return View(result.Data.Forms);
        }

        [HttpGet("templates/{templateId}/forms/statistics")]
        public async Task<IActionResult> TemplateFormsStatistics(Guid templateId)
        {
            try
            {
                var result = await _formService.GetTemplateStatisticsAsync(templateId, _currentUserId);
                return View(result);
            }
            catch
            {
                return RedirectToAction("Details", "Templates", new { id = templateId });
            }
        }
    }
}