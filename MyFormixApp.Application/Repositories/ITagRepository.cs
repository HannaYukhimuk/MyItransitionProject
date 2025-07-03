using MyFormixApp.Domain.Entities;

namespace MyFormixApp.Application.Repositories
{
    public interface ITagRepository : IBaseRepository<Tag>
    {
        Task<Tag> GetOrCreateAsync(string name);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
        Task<int> GetUsageCountAsync(Guid tagId);
        Task<IEnumerable<Tag>> SearchTagsAsync(string term, int limit);
    }
}