using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Repositories
{
    public class LikeRepository(AppDbContext context) : ILikeRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<bool> ExistsAsync(Guid userId, Guid templateId)
            => await _context.Likes
                .AnyAsync(l => l.UserId == userId && l.TemplateId == templateId);

        public async Task<int> CountAsync(Guid templateId)
            => await _context.Likes
                .CountAsync(l => l.TemplateId == templateId);

        public async Task<Like> AddAsync(Like like)
        {
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task RemoveAsync(Guid userId, Guid templateId)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.TemplateId == templateId);
            
            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
        }
    }
}