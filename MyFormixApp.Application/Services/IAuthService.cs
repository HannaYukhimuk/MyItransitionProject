using MyFormixApp.Domain.DTOs.Users;
using MyFormixApp.Domain.DTOs.Account;
using MyFormixApp.Domain.Entities;

namespace MyFormixApp.Application.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto model);
    }
}