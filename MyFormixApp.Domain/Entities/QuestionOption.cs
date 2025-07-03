namespace MyFormixApp.Domain.Entities
{
    public class QuestionOption
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public Question? Question { get; set; }
        public required string Text { get; set; }
        public int Position { get; set; }
    }
}