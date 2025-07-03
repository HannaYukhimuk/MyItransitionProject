namespace MyFormixApp.Domain.DTOs.Comments
{
    public class CommentDetailsDto
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? ParentCommentId { get; set; }
        public List<CommentDetailsDto> Replies { get; set; } = new();
    }
}