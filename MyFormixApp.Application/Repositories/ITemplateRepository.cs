using MyFormixApp.Domain.Entities;

namespace MyFormixApp.Application.Repositories
{
    public interface ITemplateRepository : IBaseRepository<Template>
    {
        Task<Template?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Template>> GetUserTemplatesAsync(Guid userId);
        Task<IEnumerable<Template>> GetPublicTemplatesAsync();
        Task<int> GetFormsCountAsync(Guid templateId);
        Task<int> GetLikesCountAsync(Guid templateId);
        Task<IEnumerable<Template>> GetPublicTemplatesByTagAsync(string tagName);
        Task<IEnumerable<Template>> GetAllTemplatesAsync();
        Task<IEnumerable<Template>> GetSharedTemplatesAsync(Guid userId);
        Task<IEnumerable<Template>> SearchByTitleAsync(string title);
        Task<IEnumerable<Template>> SearchByTagAsync(string tagName);
        Task<IEnumerable<Template>> GetPublicTemplatesPagedAsync(int page, int pageSize);
        Task<int> GetPublicTemplatesCountAsync();
        Task<List<Template>> GetUserTemplatesPagedAsync(Guid userId, int page, int pageSize);
        Task<int> GetUserTemplatesCountAsync(Guid userId);
    }
}