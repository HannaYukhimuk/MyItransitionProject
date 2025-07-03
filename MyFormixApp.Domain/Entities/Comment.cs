namespace MyFormixApp.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public required string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Guid TemplateId { get; set; }
        public Template? Template { get; set; }
        
        public Guid UserId { get; set; }
        public User? User { get; set; }
        
        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}