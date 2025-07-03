using FluentValidation;
using MyFormixApp.Domain.DTOs.Forms;
using MyFormixApp.Domain.DTOs.Answers;

namespace MyFormixApp.Application.Validators
{
    public class FormValidator : AbstractValidator<FormDto>
    {
        public FormValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty().WithMessage("Template ID is required");

            RuleFor(x => x.Answers)
                .NotEmpty().WithMessage("At least one answer is required");

            RuleForEach(x => x.Answers).ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionId)
                    .NotEmpty().WithMessage("Question ID is required");

                answer.RuleFor(a => a)
                    .Must(HaveValidValue).WithMessage("Answer value must match question type");
            });
        }

        private bool HaveValidValue(AnswerDto answer)
        {
            return answer.TextValue != null || answer.NumberValue != null || answer.BoolValue != null;
        }
    }
}