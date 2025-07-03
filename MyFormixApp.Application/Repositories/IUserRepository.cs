using MyFormixApp.Domain.Entities;

namespace MyFormixApp.Application.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameOrEmailAsync(string username, string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> MakeAdminAsync(Guid userId);
        Task<bool> IsUserInRoleAsync(Guid userId, string role);
        Task<List<User>> GetByIdsAsync(List<Guid> ids);
        Task<bool> RemoveAdminAsync(Guid userId);
        Task<User?> GetByResetTokenAsync(string token);
        Task<bool> ExistsAsync(Guid userId);
    }
}