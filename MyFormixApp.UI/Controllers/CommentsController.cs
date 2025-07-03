using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.DTOs.Comments;
using FluentValidation;
using System.Security.Claims;

namespace MyFormixApp.UI.Controllers;

[Authorize]
public class CommentsController : Controller
{
    private readonly ICommentService _commentService;
    private readonly IValidator<CommentDto> _validator;

    public CommentsController(ICommentService commentService, IValidator<CommentDto> validator)
    {
        _commentService = commentService;
        _validator = validator;
    }

    private Guid CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    [AllowAnonymous]
    [HttpGet("/templates/{templateId}/comments")]
    public async Task<IActionResult> ByTemplate(Guid templateId) =>
        PartialView("_CommentList", await _commentService.GetByTemplateAsync(templateId));

    [HttpPost("Comments/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CommentCreateDto dto)
    {
        var commentDto = new CommentDto
        {
            TemplateId = dto.TemplateId,
            Text = dto.Text,
            ParentCommentId = dto.ParentCommentId
        };

        if (!await IsValid(commentDto))
            return RedirectToTemplate(dto.TemplateId, success: false, message: "Invalid comment data");

        var result = await _commentService.TryCreateAsync(commentDto, CurrentUserId);

        return RedirectToTemplate(dto.TemplateId, result.IsSuccess, result.Message);
    }

    [HttpGet("/templates/{templateId}/comments/{id}/edit")]
    public async Task<IActionResult> Edit(Guid templateId, Guid id)
    {
        var comment = await _commentService.GetByIdAsync(id);
        if (comment == null || comment.UserId != CurrentUserId)
            return RedirectToTemplate(templateId, success: false, message: "Комментарий не найден или у вас нет прав.");

        return View(new CommentDto
        {
            Id = comment.Id,
            TemplateId = comment.TemplateId,
            Text = comment.Text,
            ParentCommentId = comment.ParentCommentId
        });
    }

    [HttpPost("/templates/{templateId}/comments/{id}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid templateId, Guid id, CommentDto dto)
    {
        dto.TemplateId = templateId;

        if (!await IsValid(dto, addModelErrors: true))
            return View(dto);

        var updated = await _commentService.UpdateAsync(id, dto, CurrentUserId);
        return RedirectToTemplate(templateId, updated != null, "Комментарий не найден или нет доступа.");
    }

    [HttpPost("/templates/{templateId}/comments/{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid templateId, Guid id)
    {
        var success = await _commentService.DeleteAsync(id, CurrentUserId);
        return RedirectToTemplate(templateId, success, "Не удалось удалить комментарий.");
    }

    private async Task<bool> IsValid(CommentDto dto, bool addModelErrors = false)
    {
        var result = await _validator.ValidateAsync(dto);
        if (result.IsValid) return true;

        if (addModelErrors)
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

        return false;
    }

    private IActionResult RedirectToTemplate(Guid templateId, bool success = true, string? message = null)
    {
        if (!string.IsNullOrEmpty(message))
            TempData[success ? "Success" : "Error"] = message;

        return RedirectToAction("View", "TemplateViews", new { id = templateId });
    }
}
