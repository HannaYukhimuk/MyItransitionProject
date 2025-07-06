using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Repositories
{
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        public TagRepository(AppDbContext context) : base(context) { }

        public async Task<Tag> GetOrCreateAsync(string name)
        {
            var normalizedName = Normalize(name);

            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedName);

            if (tag == null)
            {
                tag = new Tag { Name = name.Trim() };
                await _context.Tags.AddAsync(tag);
                await _context.SaveChangesAsync();
            }

            return tag;
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count)
        {
            return await _context.Tags
                .OrderByDescending(t => t.Templates.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUsageCountAsync(Guid tagId)
        {
            return await _context.TemplateTags
                .CountAsync(tt => tt.TagId == tagId);
        }

        public async Task<IEnumerable<Tag>> SearchTagsAsync(string term, int limit = 10)
        {
            var normalized = term.Trim();
            return await _context.Tags
                .Where(t => EF.Functions.Like(t.Name, $"%{normalized}%"))
                .OrderBy(t => t.Name)
                .Take(limit)
                .ToListAsync();
        }

        private string Normalize(string name) => name.Trim().ToLower();
    }
}
