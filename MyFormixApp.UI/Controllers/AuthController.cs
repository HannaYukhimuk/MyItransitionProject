using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.DTOs.Account;
using MyFormixApp.Domain.DTOs.Users;
using Microsoft.AspNetCore.Authorization;

namespace MyFormixApp.UI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        [HttpGet] public IActionResult Register() => View(new UserDto { Role = "User" });

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterAdmin() => View("Register", new UserDto { Role = "Admin" });

        [HttpPost]
        public async Task<IActionResult> Register(UserDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _authService.RegisterAndSignInAsync(model, HttpContext);
            return result.Success ? RedirectToAction(result.RedirectAction) : View(model);
        }

        [HttpGet] public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(UserDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _authService.LoginAndSignInAsync(model, HttpContext);
            return result.Success ? RedirectToAction(result.RedirectAction, result.RedirectController) : View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet] public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _authService.ForgotPasswordAsync(model.Email);
            return result ? RedirectToAction("Login") : View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token) => 
            string.IsNullOrEmpty(token) ? RedirectToAction("Login") : View(new ResetPasswordDto { Token = token });

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _authService.ResetPasswordAsync(model);
            return result ? RedirectToAction("Login") : View(model);
        }
    }
}