using MyFormixApp.Domain.DTOs.Answers;

namespace MyFormixApp.Domain.DTOs.Forms
{
    public class FormDetailsDto
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<AnswerDetailsDto> Answers { get; set; } = new();
        public string TemplateTitle { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}