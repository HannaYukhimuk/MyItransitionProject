using MyFormixApp.Domain.DTOs.Users;

namespace MyFormixApp.Application.Services
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(Guid id);
        Task<UserDto> GetByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> CreateAsync(UserDto userDto);
        Task<UserDto> UpdateAsync(UserDto userDto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> MakeAdminAsync(Guid userId);
        Task<bool> RemoveAdminAsync(Guid userId);
    }
}