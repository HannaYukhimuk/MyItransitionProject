using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using System;
using System.Threading.Tasks;

namespace MyFormixApp.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("{userId}/make-admin")]
        public async Task<IActionResult> MakeAdmin(Guid userId)
        {
            return await ToggleAdminAsync(userId, true, "granted");
        }

        [HttpPost("{userId}/remove-admin")]
        public async Task<IActionResult> RemoveAdmin(Guid userId)
        {
            return await ToggleAdminAsync(userId, false, "removed");
        }

        private async Task<IActionResult> ToggleAdminAsync(Guid userId, bool isGranting, string action)
        {
            var result = isGranting
                ? await _userService.MakeAdminAsync(userId)
                : await _userService.RemoveAdminAsync(userId);

            return result
                ? Ok(new { Message = $"Admin privileges {action} for user." })
                : NotFound();
        }


        [HttpPost("delete/{userId}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteUser(Guid userId)
{
    var result = await _userService.DeleteAsync(userId);
    
    if (result)
    {
        return Ok(); // или верните что-то полезное
    }
    
    return NotFound();
}
    }
}
