namespace MyFormixApp.Domain.DTOs.Questions
{
    public class QuestionOptionDto
    {
        public Guid? Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Position { get; set; }
    }
}