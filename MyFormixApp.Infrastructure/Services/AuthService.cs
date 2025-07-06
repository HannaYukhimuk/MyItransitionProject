using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyFormixApp.Application.Results;
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
        private readonly IValidator<UserDto> _userValidator;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration,
            IEmailService emailService,
            IValidator<UserDto> userValidator)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
            _userValidator = userValidator;
        }

        public async Task<AuthResult> RegisterAndSignInAsync(UserDto request, HttpContext httpContext)
        {
            var validationResult = await _userValidator.ValidateAsync(request);
            if (!validationResult.IsValid) return CreateValidationErrorResult(validationResult);
            try
            {
                var user = await RegisterUserAsync(request);
                return request.Role == "Admin" 
                    ? await AdminSignInAsync(request, user, httpContext) 
                    : new AuthResult { Success = true, RedirectAction = "Login" };
            }
            catch (ApplicationException ex)
            {
                return new AuthResult { ErrorMessage = ex.Message };
            }
        }

        public async Task<AuthResult> LoginAndSignInAsync(UserDto request, HttpContext httpContext)
        {
            var validationResult = await _userValidator.ValidateAsync(request, 
                opts => opts.IncludeProperties(x => x.Username, x => x.Password));
            if (!validationResult.IsValid) return CreateValidationErrorResult(validationResult);
            var token = await LoginAsync(request);
            if (token == null) return new AuthResult { ErrorMessage = "Incorrect login or password." };
            var user = await GetUserByUsernameAsync(request.Username);
            if (user == null) return new AuthResult { ErrorMessage = "User not found." };
            await SignInUserAsync(user.Username, user.Role, user.Id, token.AccessToken, httpContext);
            return CreateSuccessAuthResult(user.Role);
        }

        private async Task<User> RegisterUserAsync(UserDto request)
        {
            if (await _userRepository.GetByUsernameOrEmailAsync(request.Username, request.Email) != null)
                throw new ApplicationException("Username or email already exists");

            return await _userRepository.CreateAsync(CreateUserFromRequest(request));
        }

        private User CreateUserFromRequest(UserDto request) => new()
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(null!, request.Password),
            Role = request.Role ?? "User"
        };

        private async Task<AuthResult> AdminSignInAsync(UserDto request, User user, HttpContext httpContext)
        {
            var token = await LoginAsync(request);
            if (token == null) return new AuthResult { ErrorMessage = "Login error after registration" };

            await SignInUserAsync(request.Username, "Admin", user.Id, token.AccessToken, httpContext);
            return CreateSuccessAuthResult("Admin");
        }

        private async Task SignInUserAsync(string username, string role, Guid userId, string token, HttpContext httpContext)
        {
            var claims = CreateClaimsList(username, role, userId, token);
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProperties = CreateAuthProperties();

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }

        private List<Claim> CreateClaimsList(string username, string role, Guid userId, string token) => new()
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("JWT", token)
        };

        private AuthenticationProperties CreateAuthProperties() => new()
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.Username, request.Email);
            if (user == null || !user.IsActive) return null;

            return IsPasswordValid(user, request.Password) 
                ? await CreateTokenResponse(user) 
                : null;
        }

        private bool IsPasswordValid(User user, string password) => 
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            var tokenResponse = CreateTokenResponseDto(user);
            await UpdateUserRefreshToken(user, tokenResponse.RefreshToken);
            return tokenResponse;
        }

        private TokenResponseDto CreateTokenResponseDto(User user) => new()
        {
            AccessToken = CreateJwtToken(user),
            RefreshToken = GenerateToken()
        };

        private async Task UpdateUserRefreshToken(User user, string refreshToken)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);
        }

        private string CreateJwtToken(User user) =>
            new JwtSecurityTokenHandler().WriteToken(CreateSecurityToken(user));

        private JwtSecurityToken CreateSecurityToken(User user) => new(
            issuer: _configuration["AppSettings:Issuer"],
            audience: _configuration["AppSettings:Audience"],
            claims: GetUserClaims(user),
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: GetSigningCredentials());

        private IEnumerable<Claim> GetUserClaims(User user) => new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        private SigningCredentials GetSigningCredentials() => new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!)),
            SecurityAlgorithms.HmacSha512);

        public async Task<User?> GetUserByUsernameAsync(string username) => 
            await _userRepository.GetByUsernameOrEmailAsync(username, null);

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive) return false;

            await SetPasswordResetToken(user);
            await SendPasswordResetEmail(user.Email, user.ResetPasswordToken!);
            return true;
        }

        private async Task SetPasswordResetToken(User user)
        {
            user.ResetPasswordToken = GenerateToken();
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _userRepository.UpdateAsync(user);
        }

        private async Task SendPasswordResetEmail(string email, string token)
        {
            var resetLink = $"{_configuration["AppSettings:ClientUrl"]}/auth/resetpassword?token={token}";
            var message = $"Please reset your password by clicking <a href='{resetLink}'>here</a>.";
            await _emailService.SendEmailAsync(email, "Reset Password", message);
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userRepository.GetByResetTokenAsync(model.Token);
            if (user == null || user.ResetPasswordTokenExpiry < DateTime.UtcNow) return false;

            await UpdateUserPassword(user, model.NewPassword);
            return true;
        }

        private async Task UpdateUserPassword(User user, string newPassword)
        {
            user.PasswordHash = _passwordHasher.HashPassword(null!, newPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;
            await _userRepository.UpdateAsync(user);
        }

        private static string GenerateToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomNumber = new byte[32];
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber).Replace("/", "_").Replace("+", "-");
        }

        private AuthResult CreateValidationErrorResult(FluentValidation.Results.ValidationResult validationResult) => new()
        {
            ErrorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
        };

        private AuthResult CreateSuccessAuthResult(string role) => new()
        {
            Success = true,
            RedirectAction = "Index",
            RedirectController = role == "Admin" ? "Admin" : "Home"
        };
    }
}