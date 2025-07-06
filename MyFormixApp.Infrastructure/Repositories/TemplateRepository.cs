using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Repositories
{
    public class TemplateRepository : BaseRepository<Template>, ITemplateRepository
    {
        public TemplateRepository(AppDbContext context) : base(context) { }

        private IQueryable<Template> IncludeBasicDetails(IQueryable<Template> query)
        {
            return query
                .Include(t => t.User)
                .Include(t => t.Theme)
                .Include(t => t.Tags).ThenInclude(tt => tt.Tag);
        }

        private IQueryable<Template> IncludeFullDetails(IQueryable<Template> query)
        {
            return IncludeBasicDetails(query)
                .Include(t => t.Questions).ThenInclude(q => q.Options)
                .Include(t => t.Comments).ThenInclude(c => c.User)
                .Include(t => t.Accesses);
        }

        private void EnsureGuids(Template template)
        {
            if (template.Id == Guid.Empty)
                template.Id = Guid.NewGuid();

            foreach (var question in template.Questions)
            {
                if (question.Id == Guid.Empty)
                    question.Id = Guid.NewGuid();

                foreach (var option in question.Options)
                {
                    if (option.Id == Guid.Empty)
                        option.Id = Guid.NewGuid();
                }
            }
        }

        private IQueryable<Template> FilterTemplates(bool isPublic = false, Guid? userId = null)
        {
            var query = _context.Templates.AsQueryable();

            if (isPublic)
                query = query.Where(t => t.IsPublic);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            return query;
        }

        public async Task<Template?> GetByIdWithDetailsAsync(Guid id)
        {
            return await IncludeFullDetails(_context.Templates)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Template>> GetUserTemplatesAsync(Guid userId)
            => await FilterTemplates(userId: userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Template>> GetPublicTemplatesAsync()
            => await FilterTemplates(isPublic: true)
                .OrderByDescending(t => t.CreatedAt)
                .Take(100)
                .ToListAsync();

        public override async Task<Template> CreateAsync(Template template)
        {
            EnsureGuids(template);
            return await base.CreateAsync(template);
        }

        public async Task<IEnumerable<Template>> GetSharedTemplatesAsync(Guid userId)
        {
            return await _context.Templates
                .Include(t => t.Theme)
                .Include(t => t.Tags).ThenInclude(tt => tt.Tag)
                .Include(t => t.User)
                .Include(t => t.Accesses)
                .Where(t => !t.IsPublic &&
                            t.Accesses.Any(a => a.UserId == userId) &&
                            t.UserId != userId)
                .ToListAsync();
        }

        public override async Task DeleteAsync(Guid id)
        {
            var template = await _context.Templates
                .Include(t => t.Questions).ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template != null)
            {
                _context.Templates.Remove(template);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetFormsCountAsync(Guid templateId)
            => await _context.Forms.CountAsync(f => f.TemplateId == templateId);

        public async Task<int> GetLikesCountAsync(Guid templateId)
            => await _context.Likes.CountAsync(l => l.TemplateId == templateId);

        public async Task<IEnumerable<Template>> GetPublicTemplatesByTagAsync(string tagName)
        {
            return await FilterTemplates(isPublic: true)
                .Where(t => t.Tags.Any(tt => tt.Tag.Name == tagName))
                .Include(t => t.User)
                .Include(t => t.Theme)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> SearchByTitleAsync(string title)
        {
            return await FilterTemplates(isPublic: true)
                .Where(t => EF.Functions.Like(t.Title, $"%{title}%"))
                .Include(t => t.User)
                .Include(t => t.Tags).ThenInclude(tt => tt.Tag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> SearchByTagAsync(string tagName)
        {
            return await FilterTemplates(isPublic: true)
                .Where(t => t.Tags.Any(tt => tt.Tag.Name == tagName))
                .Include(t => t.User)
                .Include(t => t.Tags).ThenInclude(tt => tt.Tag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> GetAllTemplatesAsync()
        {
            return await IncludeBasicDetails(_context.Templates)
                .Include(t => t.Questions)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> GetPublicTemplatesPagedAsync(int page, int pageSize)
        {
            return await IncludeBasicDetails(FilterTemplates(isPublic: true))
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetPublicTemplatesCountAsync()
            => await FilterTemplates(isPublic: true).CountAsync();

        public async Task<List<Template>> GetUserTemplatesPagedAsync(Guid userId, int page, int pageSize)
        {
            return await IncludeBasicDetails(FilterTemplates(userId: userId))
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUserTemplatesCountAsync(Guid userId)
        {
            return await FilterTemplates(userId: userId).CountAsync();
        }
    }
}
