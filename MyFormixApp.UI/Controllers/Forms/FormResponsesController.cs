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
        private readonly ITemplateService _templateService;

        public FormResponsesController(IFormService formService, ITemplateService templateService)
        {
            _formService = formService;
            _templateService = templateService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Take(Guid id)
        {
            try
            {
                if (await _formService.GetByUserAndTemplateAsync(CurrentUserId, id) != null)
                {
                    TempData["Error"] = "You have already submitted a response for this template";
                    return RedirectToAction("View", "TemplateViews", new { id });
                }

                var responses = Request.Form
                    .Where(k => k.Key.StartsWith("responses[") && k.Key.EndsWith("]"))
                    .Select(k => new
                    {
                        QuestionId = Guid.TryParse(k.Key[10..^1], out var qId) ? qId : Guid.Empty,
                        Value = string.Join(";", k.Value.ToArray())
                    })
                    .Where(r => r.QuestionId != Guid.Empty)
                    .ToDictionary(r => r.QuestionId, r => r.Value);

                var result = await _formService.CreateResponseAsync(id, CurrentUserId, responses);
                TempData[result ? "Success" : "Error"] = result
                    ? "Your responses have been saved!"
                    : "Failed to save your responses.";

                return RedirectToAction("View", "TemplateViews", new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("View", "TemplateViews", new { id });
            }
        }

        [HttpGet("templates/{templateId}/forms")]
        public async Task<IActionResult> TemplateForms(Guid templateId)
        {
            try
            {
                var template = await _templateService.GetByIdAsync(templateId, CurrentUserId);
                if (template == null) return NotFound();

                ViewBag.TemplateTitle = template.Title;
                ViewBag.TemplateId = template.Id;

                var forms = await _formService.GetByTemplateAsync(templateId, CurrentUserId);
                return View(forms);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Templates", new { id = templateId });
            }
        }

        [HttpGet("templates/{templateId}/forms/statistics")]
        public async Task<IActionResult> TemplateFormsStatistics(Guid templateId)
        {
            try
            {
                var template = await _templateService.GetByIdAsync(templateId, CurrentUserId);
                if (template == null) return NotFound();

                ViewBag.TemplateTitle = template.Title;
                var statistics = await _formService.GetTemplateStatisticsAsync(templateId, CurrentUserId);
                return View(statistics);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Templates", new { id = templateId });
            }
        }
    }
}
