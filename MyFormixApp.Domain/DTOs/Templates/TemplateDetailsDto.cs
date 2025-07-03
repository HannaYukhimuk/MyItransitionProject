using MyFormixApp.Domain.DTOs.Comments;
using MyFormixApp.Domain.DTOs.Users;

namespace MyFormixApp.Domain.DTOs.Templates
{
    public class TemplateDetailsDto : TemplateDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserDto? Author { get; set; }
        public TemplateThemeDto? Theme { get; set; }
        public int FormsCount { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<CommentDetailsDto> Comments { get; set; } = new();
        public List<TemplateAccessDto> Accesses { get; set; } = new();
    }


}