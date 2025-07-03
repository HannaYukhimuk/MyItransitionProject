using MyFormixApp.Domain.DTOs.Tags;

namespace MyFormixApp.Application.Services
{
    public interface ITagService
    {
        Task<IEnumerable<TagCloudItemDto>> GetTagCloudAsync(int maxTags = 50);
        Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count);
        Task<IEnumerable<TagDto>> SearchTagsAsync(string term, int limit);
    }
}