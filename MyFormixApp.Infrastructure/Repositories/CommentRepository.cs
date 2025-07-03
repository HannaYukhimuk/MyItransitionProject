using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Repositories
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext context) : base(context) { }

        public override async Task<Comment?> GetByIdAsync(Guid id)
            => await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<bool> ExistsAsync(Guid commentId)
            => await _context.Comments.AnyAsync(c => c.Id == commentId);

        public async Task<IEnumerable<Comment>> GetByTemplateWithRepliesAsync(Guid templateId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.TemplateId == templateId)
                .AsSplitQuery()
                .ToListAsync();
        }
    }
}