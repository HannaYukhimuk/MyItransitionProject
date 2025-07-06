using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using MyFormixApp.Application.Validators;
using MyFormixApp.Application.Mapping;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.UI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomLocalization();
builder.Services.AddRazorPages();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(GetConnectionString(builder.Configuration)));

builder.Services.AddAutoMapper(
    typeof(AnswerProfile),
    typeof(FormProfile),
    typeof(TemplateProfile),
    typeof(CommentProfile),
    typeof(UserProfile));

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddMyFormixServices(builder.Configuration)
    .AddCustomAuthentication()
    .AddCustomAuthorization();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Migration failed!");
    throw;
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCustomLocalization(); 
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
    return $"Host=dpg-d1jau33e5dus73d43hhg-a;Port=5432;Database=myformixapp_o1to;Username=myformixapp_o1to_user;Password=Nn5HwBM735DYzuKltyL8WpwKRlVExLSI";
}