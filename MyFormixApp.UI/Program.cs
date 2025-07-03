using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using MyFormixApp.Application.Repositories;
using MyFormixApp.Application.Services;
using MyFormixApp.Domain.Entities;
using FluentValidation;
using FluentValidation.AspNetCore;
using MyFormixApp.Application.Validators;
using MyFormixApp.Domain.DTOs.Templates;
using MyFormixApp.Domain.DTOs.Comments;
using MyFormixApp.Domain.DTOs.Questions;
using MyFormixApp.Domain.DTOs.Tags;
using MyFormixApp.Domain.DTOs.Forms;
using MyFormixApp.Application.Mapping;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Infrastructure.Repositories;
using MyFormixApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();

builder.Services.AddSingleton(_ => 
{
    var connectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Azure Blob Storage connection string is not configured");
    }
    return new BlobServiceClient(connectionString);
});

var connectionString = GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(connectionString));

builder.Services.AddAutoMapper(
       typeof(AnswerProfile),
       typeof(FormProfile),
       typeof(TemplateProfile),
       typeof(CommentProfile),
       typeof(UserProfile)
);

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
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

// Настройка авторизации с политиками
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("RequireUserRole", policy => 
        policy.RequireRole("User", "Admin"));
});

// Регистрация репозиториев
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<ITemplateThemeRepository, TemplateThemeRepository>();
builder.Services.AddScoped<IFormRepository, FormRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

// Регистрация сервисов
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<ITemplateThemeService, TemplateThemeService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICloudStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IEmailService, EmailService>();

// Регистрация валидаторов
builder.Services.AddScoped<IValidator<TemplateDto>, TemplateValidator>();
builder.Services.AddScoped<IValidator<TemplateThemeDto>, TemplateThemeValidator>();
builder.Services.AddScoped<IValidator<QuestionDto>, QuestionValidator>();
builder.Services.AddScoped<IValidator<FormDto>, FormValidator>();
builder.Services.AddScoped<IValidator<TagDto>, TagValidator>();
builder.Services.AddScoped<IValidator<CommentDto>, CommentValidator>();

var app = builder.Build();

try 
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Migration failed!");
    throw;  
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();
app.MapRazorPages();

app.Run();



string GetConnectionString(IConfiguration config)
{
    var host = config["DB_HOST"];
    var port = config["DB_PORT"];
    var dbName = config["DB_NAME"];
    var dbUser = config["DB_USER"];
    var dbPassword = config["DB_PASSWORD"];
    
    return $"Host=dpg-d1jau33e5dus73d43hhg-a;Port=5432;Database=myformixapp_o1to;Username=myformixapp_o1to_user;Password=Nn5HwBM735DYzuKltyL8WpwKRlVExLSI";
}