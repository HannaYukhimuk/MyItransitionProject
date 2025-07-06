using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Repositories
{
    public class FormRepository : BaseRepository<Form>, IFormRepository
    {
        public FormRepository(AppDbContext context) : base(context) { }

        private IQueryable<Form> IncludeFormDetails(IQueryable<Form> query)
        {
            return query
                .Include(f => f.Template)
                .Include(f => f.User)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                        .ThenInclude(q => q.Options);
        }

        public async Task<Form?> GetByIdWithDetailsAsync(Guid id)
        {
            return await IncludeFormDetails(_context.Forms)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Form>> GetByUserAsync(Guid userId)
            => await _context.Forms
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Form>> GetByTemplateAsync(Guid templateId)
            => await _context.Forms
                .Where(f => f.TemplateId == templateId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Form>> GetAllWithDetailsAsync()
        {
            return await IncludeFormDetails(_context.Forms)
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Form>> GetByTemplateWithDetailsAsync(Guid templateId)
        {
            return await IncludeFormDetails(
                    _context.Forms.Where(f => f.TemplateId == templateId))
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Form?> GetByUserAndTemplateAsync(Guid userId, Guid templateId)
            => await _context.Forms
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TemplateId == templateId);
    }
}
