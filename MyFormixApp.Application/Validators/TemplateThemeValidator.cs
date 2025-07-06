using FluentValidation;
using MyFormixApp.Domain.DTOs.Templates;

namespace MyFormixApp.Application.Validators
{
    public class TemplateThemeValidator : AbstractValidator<TemplateThemeDto>
    {
        public TemplateThemeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Theme name is required")
                .MaximumLength(50).WithMessage("Theme name cannot exceed 50 characters");
        }
    }
}