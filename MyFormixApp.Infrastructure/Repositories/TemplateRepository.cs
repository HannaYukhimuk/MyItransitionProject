using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Repositories
{
    public class TemplateRepository : BaseRepository<Template>, ITemplateRepository
    {
        public TemplateRepository(AppDbContext context) : base(context) { }

        public async Task<Template?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Templates
                .Include(t => t.Accesses)
                .Include(t => t.User)
                .Include(t => t.Theme)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Template>> GetUserTemplatesAsync(Guid userId)
            => await _context.Templates
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Template>> GetPublicTemplatesAsync()
            => await _context.Templates
                .Where(t => t.IsPublic)
                .OrderByDescending(t => t.CreatedAt)
                .Take(100)
                .ToListAsync();

        public override async Task<Template> CreateAsync(Template template)
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

            return await base.CreateAsync(template);
        }

        public async Task<IEnumerable<Template>> GetSharedTemplatesAsync(Guid userId)
        {
            return await _context.Templates
                .Include(t => t.Theme)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
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
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
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
            return await _context.Templates
                .Where(t => t.IsPublic && t.Tags.Any(tt => tt.Tag.Name == tagName))
                .Include(t => t.User)
                .Include(t => t.Theme)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> SearchByTitleAsync(string title)
        {
            return await _context.Templates
                .Where(t => t.IsPublic && EF.Functions.Like(t.Title, $"%{title}%"))
                .Include(t => t.User)
                .Include(t => t.Tags).ThenInclude(tt => tt.Tag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> SearchByTagAsync(string tagName)
        {
            return await _context.Templates
                .Where(t => t.IsPublic && t.Tags.Any(tt => tt.Tag.Name == tagName))
                .Include(t => t.User)
                .Include(t => t.Tags).ThenInclude(tt => tt.Tag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> GetAllTemplatesAsync()
        {
            return await _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.Tags).ThenInclude(tt => tt.Tag)
                .Include(t => t.User)
                .Include(t => t.Theme)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Template> UpdateAsync(Template template)
        {
            _context.Templates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<IEnumerable<Form>> GetFormsForTemplateAsync(Guid templateId)
        {
            return await _context.Forms
                .Where(f => f.TemplateId == templateId)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> GetPublicTemplatesPagedAsync(int page, int pageSize)
        {
            return await _context.Templates
                .Where(t => t.IsPublic)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.User)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .ToListAsync();
        }

        public async Task<int> GetPublicTemplatesCountAsync()
            => await _context.Templates.CountAsync(t => t.IsPublic);



        public IQueryable<Template> GetUserTemplatesQueryable(Guid userId)
        {
            return _context.Templates
                .Where(t => t.UserId == userId)
                .Include(t => t.User)
                .Include(t => t.Tags)



                    .ThenInclude(tt => tt.Tag);
        }




// TemplateRepository.cs
public async Task<List<Template>> GetUserTemplatesPagedAsync(Guid userId, int page, int pageSize)
{
    return await _context.Templates
        .Where(t => t.UserId == userId)
        .Include(t => t.User)
        .Include(t => t.Tags)
            .ThenInclude(tt => tt.Tag)
        .OrderByDescending(t => t.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}

public async Task<int> GetUserTemplatesCountAsync(Guid userId)
{
    return await _context.Templates
        .Where(t => t.UserId == userId)
        .CountAsync();
}
    }
}