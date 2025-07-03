using AutoMapper;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Comments;

namespace MyFormixApp.Application.Mapping
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty));

            CreateMap<Comment, CommentDetailsDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty));

        }
    }
}