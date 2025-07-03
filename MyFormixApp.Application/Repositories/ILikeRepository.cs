using MyFormixApp.Domain.Entities;

namespace MyFormixApp.Application.Repositories
{
    public interface ILikeRepository
    {
        Task<bool> ExistsAsync(Guid userId, Guid templateId);
        Task<int> CountAsync(Guid templateId);
        Task<Like> AddAsync(Like like);
        Task RemoveAsync(Guid userId, Guid templateId);
    }
}