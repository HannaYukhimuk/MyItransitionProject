using MyFormixApp.Domain.DTOs.Users;
using MyFormixApp.Domain.DTOs.Account;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Application.Results;
using Microsoft.AspNetCore.Http;

namespace MyFormixApp.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAndSignInAsync(UserDto request, HttpContext httpContext);
        Task<AuthResult> LoginAndSignInAsync(UserDto request, HttpContext httpContext);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto model);
    }
}
