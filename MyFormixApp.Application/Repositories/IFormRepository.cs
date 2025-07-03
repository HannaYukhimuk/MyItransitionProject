using MyFormixApp.Domain.Entities;

namespace MyFormixApp.Application.Repositories
{
    public interface IFormRepository : IBaseRepository<Form>
    {
        Task<Form?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Form>> GetByUserAsync(Guid userId);
        Task<IEnumerable<Form>> GetByTemplateAsync(Guid templateId);
        Task<IEnumerable<Form>> GetAllWithDetailsAsync();
        Task<IEnumerable<Form>> GetByTemplateWithDetailsAsync(Guid templateId);
        Task<Form?> GetByUserAndTemplateAsync(Guid userId, Guid templateId);
    }
}