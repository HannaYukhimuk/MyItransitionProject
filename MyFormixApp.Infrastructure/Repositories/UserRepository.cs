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
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
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
            => await UpdateRoleAsync(userId, "Admin");

        public async Task<bool> RemoveAdminAsync(Guid userId)
            => await UpdateRoleAsync(userId, "User");

        private async Task<bool> UpdateRoleAsync(Guid userId, string role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.Role = role;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string username, string email)
            => await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetByResetTokenAsync(string token)
            => await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == token);

        public async Task<bool> ExistsAsync(Guid userId)
            => await _context.Users.AnyAsync(u => u.Id == userId);
    }
}
