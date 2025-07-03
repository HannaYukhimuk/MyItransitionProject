namespace MyFormixApp.Domain.Entities
{
    public class Form
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Template? Template { get; set; }
        public User? User { get; set; }
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }

}