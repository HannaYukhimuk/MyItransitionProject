namespace MyFormixApp.Domain.Entities
{
    public class Question
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public Template? Template { get; set; }
        public string Type { get; set; } = "text";  
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } 
        public string? ImageUrl { get; set; }
        public bool IsRequired { get; set; } = true;
        public int Position { get; set; }
        public bool ShowInTable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}