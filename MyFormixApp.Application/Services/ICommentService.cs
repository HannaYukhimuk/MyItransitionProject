using MyFormixApp.Domain.DTOs.Comments;

namespace MyFormixApp.Application.Services
{
    public interface ICommentService
    {
        Task<CommentDetailsDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<CommentDetailsDto>> GetByTemplateAsync(Guid templateId);
        Task<CommentDetailsDto> CreateAsync(CommentDto dto, Guid userId);
        Task<(bool IsSuccess, string Message)> TryCreateAsync(CommentDto dto, Guid userId); 
        Task<CommentDetailsDto?> UpdateAsync(Guid id, CommentDto dto, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}