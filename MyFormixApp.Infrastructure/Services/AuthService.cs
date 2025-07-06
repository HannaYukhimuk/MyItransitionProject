using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MyFormixApp.Application.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Users;
using MyFormixApp.Domain.DTOs.Account;
using MyFormixApp.Application.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

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

        public async Task<AuthResult> RegisterAndSignInAsync(UserDto request, HttpContext httpContext)
        {
            try
            {
                var user = await RegisterUserAsync(request);
                if (request.Role == "Admin") return await AdminSignInAsync(request, user, httpContext);                
                return new AuthResult { Success = true, RedirectAction = "Login" };
            }
            catch (ApplicationException ex)
            {
                return new AuthResult { ErrorMessage = ex.Message };
            }
        }

        public async Task<AuthResult> LoginAndSignInAsync(UserDto request, HttpContext httpContext)
        {
            var token = await LoginAsync(request);
            if (token == null) return new AuthResult { ErrorMessage = "Incorrect login or password." };
            var user = await GetUserByUsernameAsync(request.Username);
            if (user == null) return new AuthResult { ErrorMessage = "User not found." };
            await SignInUserAsync(new UserDto { Username = user.Username }, user.Role, user.Id, token.AccessToken, httpContext);
            return new AuthResult 
            { 
                Success = true, 
                RedirectAction = "Index", 
                RedirectController = user.Role == "Admin" ? "Admin" : "Home" 
            };
        }

        private async Task<User> RegisterUserAsync(UserDto request)
        {
            if (await _userRepository.GetByUsernameOrEmailAsync(request.Username, request.Email) != null)
                throw new ApplicationException("Username or email already exists");
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(null!, request.Password),
                Role = request.Role ?? "User"
            };
            return await _userRepository.CreateAsync(user);
        }

        private async Task<AuthResult> AdminSignInAsync(UserDto request, User user, HttpContext httpContext)
        {
            var token = await LoginAsync(request);
            if (token == null) return new AuthResult { ErrorMessage = "Login error after registration" };
            await SignInUserAsync(request, "Admin", user.Id, token.AccessToken, httpContext);
            return new AuthResult { Success = true, RedirectAction = "Index", RedirectController = "Admin" };
        }

        private async Task SignInUserAsync(UserDto model, string role, Guid userId, string token, HttpContext httpContext)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, model.Username),
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Role, role),
                new("JWT", token)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });
        }

        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.Username, request.Email);
            if (user is null || !user.IsActive) return null;
            return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) != PasswordVerificationResult.Failed 
                ? await CreateTokenResponse(user) 
                : null;
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

        private string CreateJwtToken(User user) =>
            new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor
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
            }));

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