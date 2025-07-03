using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using System.Security.Claims;

namespace MyFormixApp.UI.Controllers
{
    [AllowAnonymous]
    public class TemplateViewsController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly ICommentService _commentService;

        public TemplateViewsController(ITemplateService templateService, ICommentService commentService)
        {
            _templateService = templateService;
            _commentService = commentService;
        }

        private Guid? TryGetUserId() =>
            User.Identity?.IsAuthenticated == true
            ? Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!)
            : (Guid?)null;

        public async Task<IActionResult> Details(Guid id)
        {
            var template = await _templateService.GetByIdAsync(id, TryGetUserId());
            if (template == null) return NotFound();

            ViewBag.CanEdit = template.Author.Id == TryGetUserId();
            return View(template);
        }

        public async Task<IActionResult> View(Guid id)
        {
            var template = await _templateService.GetByIdAsync(id, TryGetUserId());
            if (template == null) return NotFound();

            template.Comments = (await _commentService.GetByTemplateAsync(id)).ToList();
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated == true;

            return View(template);
        }

        public async Task<IActionResult> Public()
        {
            var templates = await _templateService.GetPublicTemplatesAsync();
            return View(nameof(Index), templates);
        }

        [Authorize]
        public async Task<IActionResult> My(int page = 1, int pageSize = 8)
        {
            var userId = TryGetUserId()!.Value;
            var templates = await _templateService.GetUserTemplatesPagedAsync(userId, page, pageSize);
            var totalCount = await _templateService.GetUserTemplatesCountAsync(userId);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(templates);
        }

        [Authorize]
        [HttpGet("templates/shared")]
        public async Task<IActionResult> Shared()
        {
            var userId = TryGetUserId()!.Value;
            var templates = await _templateService.GetSharedTemplatesAsync(userId);
            return View(templates);
        }
    }

}