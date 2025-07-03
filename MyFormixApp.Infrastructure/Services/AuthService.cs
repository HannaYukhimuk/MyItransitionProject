using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Users;
using MyFormixApp.Domain.DTOs.Account;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(IUserRepository userRepository, IConfiguration configuration, IEmailService emailService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.Username, request.Email);
            if (user is null || !user.IsActive) return null;

            return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) != PasswordVerificationResult.Failed 
                ? await CreateTokenResponse(user) 
                : null;
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            if (await _userRepository.GetByUsernameOrEmailAsync(request.Username, request.Email) != null)
                throw new ApplicationException("Username or email already exists");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(null!, request.Password),
                Role = "User"
            };

            return await _userRepository.CreateAsync(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            var tokenResponse = new TokenResponseDto
            {
                AccessToken = CreateJwtToken(user),
                RefreshToken = GenerateToken()
            };

            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            return tokenResponse;
        }

        private string CreateJwtToken(User user)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!)),
                    SecurityAlgorithms.HmacSha512),
                Issuer = _configuration["AppSettings:Issuer"],
                Audience = _configuration["AppSettings:Audience"]
            };

            return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
        }

        public async Task<User?> GetUserByUsernameAsync(string username) => 
            await _userRepository.GetByUsernameOrEmailAsync(username, null);

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive) return false;

            user.ResetPasswordToken = GenerateToken();
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _userRepository.UpdateAsync(user);

            var resetLink = $"{_configuration["AppSettings:ClientUrl"]}/auth/resetpassword?token={user.ResetPasswordToken}";
            await _emailService.SendEmailAsync(email, "Reset Password", 
                $"Please reset your password by clicking <a href='{resetLink}'>here</a>.");

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userRepository.GetByResetTokenAsync(model.Token);
            if (user == null || user.ResetPasswordTokenExpiry < DateTime.UtcNow) return false;

            user.PasswordHash = _passwordHasher.HashPassword(null!, model.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;
            
            return await _userRepository.UpdateAsync(user) != null;
        }

        private static string GenerateToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber).Replace("/", "_").Replace("+", "-");
        }
    }
}
