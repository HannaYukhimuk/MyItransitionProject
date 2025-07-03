using FluentValidation;
using MyFormixApp.Domain.DTOs.Comments;

namespace MyFormixApp.Application.Validators
{
    public class CommentValidator : AbstractValidator<CommentDto>
    {
        public CommentValidator()
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Comment text is required")
                .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters");

            RuleFor(x => x.TemplateId)
                .NotEmpty().WithMessage("Template ID is required");
        }
    }
}