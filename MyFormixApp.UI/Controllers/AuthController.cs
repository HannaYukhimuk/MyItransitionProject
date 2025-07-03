using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.DTOs.Account;
using MyFormixApp.Domain.DTOs.Users;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MyFormixApp.UI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private async Task SignInUserAsync(UserDto model, string role, Guid userId, string token)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("JWT", token)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });
        }

        [HttpGet]
        public IActionResult Register() => View(new UserDto { Role = "User" });

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterAdmin() => View("Register", new UserDto { Role = "Admin" });

        [HttpPost]
public async Task<IActionResult> Register(UserDto model)
{
    if (model.Password != model.ConfirmPassword)
    {
        ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
    }

    if (!ModelState.IsValid) return View(model);

    try
    {
        var user = await _authService.RegisterAsync(model);

        if (model.Role == "Admin")
        {
            var token = await _authService.LoginAsync(model);
            if (token != null)
            {
                await SignInUserAsync(model, "Admin", user.Id, token.AccessToken);
                return RedirectToAction("Index", "Admin");
            }
        }

        TempData["Success"] = "Registration successful!";
        return RedirectToAction("Login");
    }
    catch (ApplicationException ex)
    {
        ModelState.AddModelError(string.Empty, ex.Message);
        return View(model);
    }
}

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(UserDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = await _authService.LoginAsync(model);
            if (token == null)
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                return View(model);
            }

            var user = await _authService.GetUserByUsernameAsync(model.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь не найден.");
                return View(model);
            }

            await SignInUserAsync(model, user.Role, user.Id, token.AccessToken);

            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _authService.ForgotPasswordAsync(model.Email))
            {
                TempData["Success"] = "Password reset link has been sent to your email.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "User with this email not found.");
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");
            return View(new ResetPasswordDto { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _authService.ResetPasswordAsync(model))
            {
                TempData["Success"] = "Password has been reset successfully.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Invalid or expired token.");
            return View(model);
        }
    }
}
