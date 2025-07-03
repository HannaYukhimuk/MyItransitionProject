namespace MyFormixApp.Domain.DTOs.Answers
{
    public class AnswerDetailsDto : AnswerDto
    {
        public string? QuestionTitle { get; set; }
        public string? QuestionDescription { get; set; }
        public string? QuestionType { get; set; }
        public List<string> Options { get; set; } = new();
    }
}