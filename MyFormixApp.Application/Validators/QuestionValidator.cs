using FluentValidation;
using MyFormixApp.Domain.DTOs.Questions;

namespace MyFormixApp.Application.Validators
{
    public class QuestionValidator : AbstractValidator<QuestionDto>
    {
        public QuestionValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Question title is required")
                .MaximumLength(255).WithMessage("Title cannot exceed 255 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Position)
                .GreaterThanOrEqualTo(0).WithMessage("Position must be positive");
        }

        private bool BeValidType(string type)
        {
            return type is "text" or "textarea" or "number" or "checkbox";
        }
    }
}