using MyFormixApp.Domain.DTOs.Answers;

namespace MyFormixApp.Domain.DTOs.Forms
{
    public class FormDto
    {
        public Guid? Id { get; set; }
        public Guid TemplateId { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }
}