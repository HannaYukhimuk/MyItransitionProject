namespace MyFormixApp.Domain.Entities
{
    public class Template
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int ThemeId { get; set; }
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
        public TemplateTheme? Theme { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<Form> Forms { get; set; } = new List<Form>();
        public ICollection<TemplateTag> Tags { get; set; } = new List<TemplateTag>();
        public ICollection<TemplateAccess> Accesses { get; set; } = new List<TemplateAccess>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}