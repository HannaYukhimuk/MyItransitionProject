using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Infrastructure.Data;
using MyFormixApp.Application.Repositories;

namespace MyFormixApp.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
            => await _context.Users.FindAsync(id);

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _context.Users.ToListAsync();

        public async Task<User> CreateAsync(User user)
        {
            if (user.Id == Guid.Empty)
                user.Id = Guid.NewGuid();

            user.CreatedAt = DateTime.UtcNow;
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MakeAdminAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.Role = "Admin";
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserInRoleAsync(Guid userId, string role)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Role == role;
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string username, string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        public async Task<bool> ExistsAsync(Guid userId)
            => await _context.Users.AnyAsync(u => u.Id == userId);

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<List<User>> GetByIdsAsync(List<Guid> ids)
            => await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

        public async Task<bool> RemoveAdminAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.Role = "User";
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetByResetTokenAsync(string token)
            => await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == token);
    }
}