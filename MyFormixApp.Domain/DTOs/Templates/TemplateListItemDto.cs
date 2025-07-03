using MyFormixApp.Domain.DTOs.Users;

namespace MyFormixApp.Domain.DTOs.Templates
{
    public class TemplateListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserDto? Author { get; set; }
        public string AuthorName => Author?.Username ?? "Unknown";
        public TemplateThemeDto? Theme { get; set; }
        public int QuestionsCount { get; set; }
        public int FormsCount { get; set; }
        public int LikesCount { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}