namespace MyFormixApp.Domain.DTOs.Comments
{
    public class CommentCreateDto
    {
        public Guid TemplateId { get; set; }
        public string Text { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
    }
}