namespace MyFormixApp.Domain.Entities
{
    public class Like
    {
        public Guid UserId { get; set; }
        public Guid TemplateId { get; set; }
        public User? User { get; set; }
        public Template? Template { get; set; }
    }
}