using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.DTOs.Likes;
using System.Security.Claims;

namespace MyFormixApp.UI.Controllers;

[Authorize]
public class LikesController : Controller
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    private Guid CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(LikeDto dto)
    {
        var result = await _likeService.TryToggleLikeAsync(dto, CurrentUserId);
        if (!result.IsSuccess)
            TempData["Error"] = result.Message;

        return RedirectToAction("View", "TemplateViews", new { id = dto.TemplateId });
    }
}
