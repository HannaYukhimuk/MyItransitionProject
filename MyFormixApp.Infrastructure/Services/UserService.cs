using AutoMapper;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Users;
using MyFormixApp.Application.Repositories;
using MyFormixApp.Application.Services;

namespace MyFormixApp.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> GetByIdAsync(Guid id) => 
            _mapper.Map<UserDto>(await _userRepository.GetByIdAsync(id));

        public async Task<UserDto> GetByEmailAsync(string email) => 
            _mapper.Map<UserDto>(await _userRepository.GetByEmailAsync(email));

        public async Task<IEnumerable<UserDto>> GetAllAsync() => 
            _mapper.Map<IEnumerable<UserDto>>(await _userRepository.GetAllAsync());

        public async Task<UserDto> CreateAsync(UserDto userDto) => 
            _mapper.Map<UserDto>(await _userRepository.CreateAsync(_mapper.Map<User>(userDto)));

        public async Task<UserDto> UpdateAsync(UserDto userDto) => 
            _mapper.Map<UserDto>(await _userRepository.UpdateAsync(_mapper.Map<User>(userDto)));

        public async Task<bool> DeleteAsync(Guid id) => 
            await _userRepository.DeleteAsync(id);

        public async Task<bool> MakeAdminAsync(Guid userId) => 
            await _userRepository.MakeAdminAsync(userId);

        public async Task<bool> RemoveAdminAsync(Guid userId) => 
            await _userRepository.RemoveAdminAsync(userId);
    }
}