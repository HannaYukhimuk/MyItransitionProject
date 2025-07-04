using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using MyFormixApp.Application.Repositories;
using MyFormixApp.Application.Services;
using FluentValidation;
using MyFormixApp.Application.Validators;
using MyFormixApp.Domain.DTOs.Templates;
using MyFormixApp.Domain.DTOs.Comments;
using MyFormixApp.Domain.DTOs.Questions;
using MyFormixApp.Domain.DTOs.Tags;
using MyFormixApp.Domain.DTOs.Forms;
using MyFormixApp.Infrastructure.Repositories;
using MyFormixApp.Infrastructure.Services;

namespace MyFormixApp.UI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyFormixServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITemplateRepository, TemplateRepository>();
            services.AddScoped<ITemplateThemeRepository, TemplateThemeRepository>();
            services.AddScoped<IFormRepository, FormRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<ITemplateThemeService, TemplateThemeService>();
            services.AddScoped<IFormService, FormService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<ILikeService, LikeService>();
            services.AddScoped<ICloudStorageService, AzureBlobStorageService>();
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<IEmailService, EmailService>();

            services.AddScoped<IValidator<TemplateDto>, TemplateValidator>();
            services.AddScoped<IValidator<TemplateThemeDto>, TemplateThemeValidator>();
            services.AddScoped<IValidator<QuestionDto>, QuestionValidator>();
            services.AddScoped<IValidator<FormDto>, FormValidator>();
            services.AddScoped<IValidator<TagDto>, TagValidator>();
            services.AddScoped<IValidator<CommentDto>, CommentValidator>();

            services.AddSingleton(_ =>
            {
                var connStr = config.GetConnectionString("AzureBlobStorage");
                if (string.IsNullOrEmpty(connStr))
                    throw new InvalidOperationException("Azure Blob Storage connection string is not configured");
                return new BlobServiceClient(connStr);
            });

            return services;
        }
    }
}