using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;

namespace MyFormixApp.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
            private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> ManageUsers()
    {
        var users = await _userService.GetAllAsync();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> MakeAdmin(Guid userId)
    {
        await _userService.MakeAdminAsync(userId);
        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveAdmin(Guid userId)
    {
        await _userService.RemoveAdminAsync(userId);
        return RedirectToAction(nameof(ManageUsers));
    }
        public IActionResult Index()
        {
            return View();
        }
    }
}