namespace MyFormixApp.Domain.DTOs.Questions
{
    public class QuestionDto
    {
        public Guid? Id { get; set; }
        public Guid TemplateId { get; set; }
        public string Type { get; set; } = "text";
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsRequired { get; set; } = true;
        public int Position { get; set; }
        public bool ShowInTable { get; set; } = true;
        public List<QuestionOptionDto> Options { get; set; } = new();
    }
}