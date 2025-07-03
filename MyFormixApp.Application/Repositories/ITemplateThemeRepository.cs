using MyFormixApp.Domain.Entities;

namespace MyFormixApp.Application.Repositories
{
    public interface ITemplateThemeRepository : IBaseRepository<TemplateTheme>
    {
        Task<IEnumerable<TemplateTheme>> GetAllAsync();
        Task<TemplateTheme?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}