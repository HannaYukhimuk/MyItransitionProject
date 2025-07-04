using MyFormixApp.Application.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace MyFormixApp.UI.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;

                options.Events = new CookieAuthenticationEvents
                {
                    OnSigningIn = async context =>
                    {
                        var principal = context.Principal;
                        if (principal?.Identity is ClaimsIdentity identity)
                        {
                            var username = identity.Name;
                            var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                            var user = await userRepository.GetByUsernameOrEmailAsync(username, null);

                            if (user != null)
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                            }
                        }
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));
                options.AddPolicy("RequireUserRole", policy =>
                    policy.RequireRole("User", "Admin"));
            });

            return services;
        }
    }
}