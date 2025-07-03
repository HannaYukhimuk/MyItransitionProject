namespace MyFormixApp.Domain.DTOs.Answers
{
    public class AnswerDto
    {
        public Guid? Id { get; set; }
        public Guid QuestionId { get; set; }
        public string? TextValue { get; set; }
        public List<string>? MultiTextValue { get; set; }
        public double? NumberValue { get; set; }
        public bool? BoolValue { get; set; }
    }
}