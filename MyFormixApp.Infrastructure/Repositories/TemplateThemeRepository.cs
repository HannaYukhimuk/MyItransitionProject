using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Repositories
{
    public class TemplateThemeRepository : BaseRepository<TemplateTheme>, ITemplateThemeRepository
    {
        public TemplateThemeRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<TemplateTheme>> GetAllAsync()
            => await _context.TemplateThemes.AsNoTracking().ToListAsync();

        public override async Task<TemplateTheme?> GetByIdAsync(Guid id)
            => throw new NotSupportedException("Use GetByIdAsync(int id) for TemplateTheme");

        public async Task<TemplateTheme?> GetByIdAsync(int id)
            => await _context.TemplateThemes.FindAsync(id);

        public override async Task DeleteAsync(Guid id)
            => throw new NotSupportedException("Use DeleteAsync(int id) for TemplateTheme");

        public async Task<bool> DeleteAsync(int id)
{
    var theme = await _context.TemplateThemes.FindAsync(id);
    if (theme != null)
    {
        _context.TemplateThemes.Remove(theme);
        await _context.SaveChangesAsync();
        return true;
    }
    return false;
}

    }
}