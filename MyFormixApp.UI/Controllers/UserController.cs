using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using System;
using System.Threading.Tasks;

namespace MyFormixApp.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/make-admin")]
        public async Task<IActionResult> MakeAdmin(Guid userId)
        {
            return await ToggleAdminAsync(userId, true, "granted");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/remove-admin")]
        public async Task<IActionResult> RemoveAdmin(Guid userId)
        {
            return await ToggleAdminAsync(userId, false, "removed");
        }

        [HttpGet("getIdByEmail")]
        public async Task<IActionResult> GetUserIdByEmail([FromQuery] string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
                return NotFound();

            return Ok(user.Id.ToString());
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
    }
}
