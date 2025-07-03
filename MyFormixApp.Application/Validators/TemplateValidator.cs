using FluentValidation;
using MyFormixApp.Domain.DTOs.Templates;

namespace MyFormixApp.Application.Validators
{
    public class TemplateValidator : AbstractValidator<TemplateDto>
    {
        public TemplateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.ThemeId)
                .GreaterThan(0).WithMessage("Theme is required");

            RuleFor(x => x.Tags)
                .Must(t => t.Count <= 5).WithMessage("Maximum 5 tags allowed")
                .ForEach(t => t.MaximumLength(20).WithMessage("Tag cannot exceed 20 characters"));

            RuleFor(x => x.AllowedUserIds)
                .Must(ids => ids.Count <= 50).WithMessage("Maximum 50 users allowed for private template");
        }
    }
}