using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Repositories
{
    public class FormRepository : BaseRepository<Form>, IFormRepository
    {
        public FormRepository(AppDbContext context) : base(context) { }

        public async Task<Form?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Forms
                .Include(f => f.Template)
                .Include(f => f.User)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                        .ThenInclude(q => q.Options)
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
            return await _context.Forms
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                        .ThenInclude(q => q.Options)
                .Include(f => f.Template)
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Form>> GetByTemplateWithDetailsAsync(Guid templateId)
        {
            return await _context.Forms
                .Where(f => f.TemplateId == templateId)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                        .ThenInclude(q => q.Options)
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Form?> GetByUserAndTemplateAsync(Guid userId, Guid templateId)
            => await _context.Forms
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TemplateId == templateId);
    }
}