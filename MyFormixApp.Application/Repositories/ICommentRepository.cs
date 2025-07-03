using MyFormixApp.Domain.Entities;

namespace MyFormixApp.Application.Repositories
{
    public interface ICommentRepository : IBaseRepository<Comment>
    {
        Task<bool> ExistsAsync(Guid commentId);
        Task<IEnumerable<Comment>> GetByTemplateWithRepliesAsync(Guid templateId);
    }
}