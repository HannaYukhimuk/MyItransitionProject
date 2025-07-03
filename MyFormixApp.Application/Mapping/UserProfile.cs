using AutoMapper;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Users;

namespace MyFormixApp.Application.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
            
            CreateMap<UserDto, User>();
        }
    }
}