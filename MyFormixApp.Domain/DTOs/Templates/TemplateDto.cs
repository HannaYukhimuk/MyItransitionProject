using MyFormixApp.Domain.DTOs.Questions;

namespace MyFormixApp.Domain.DTOs.Templates
{
    public class TemplateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int ThemeId { get; set; }
        public bool IsPublic { get; set; } = true;
        public List<string> Tags { get; set; } = new();
        public List<Guid> AllowedUserIds { get; set; } = new();
        public List<QuestionDto> Questions { get; set; } = new();
    }
}