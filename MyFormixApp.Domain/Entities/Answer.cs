namespace MyFormixApp.Domain.Entities
{
    public class Answer
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public Guid QuestionId { get; set; }

        public string? ValueText { get; set; }
        public int? ValueNumber { get; set; }
        public bool? ValueBool { get; set; }

        public Form? Form { get; set; }
        public Question? Question { get; set; }
    }
}