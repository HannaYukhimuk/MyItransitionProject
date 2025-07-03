using FluentValidation;
using MyFormixApp.Domain.DTOs.Tags;

namespace MyFormixApp.Application.Validators
{
    public class TagValidator : AbstractValidator<TagDto>
    {
        public TagValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tag name is required")
                .MaximumLength(20).WithMessage("Tag name cannot exceed 20 characters")
                .Matches("^[a-zA-Z0-9_\\-]+$").WithMessage("Tag can only contain letters, numbers, hyphens and underscores");
        }
    }
}