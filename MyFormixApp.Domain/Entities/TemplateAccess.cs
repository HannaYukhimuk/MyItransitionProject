namespace MyFormixApp.Domain.Entities
{
    public class TemplateAccess
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public Guid UserId { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        
        public Template Template { get; set; }
        public User User { get; set; }
    }
}