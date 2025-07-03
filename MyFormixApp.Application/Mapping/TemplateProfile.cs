using AutoMapper;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Templates;

namespace MyFormixApp.Application.Mapping
{
    public class TemplateProfile : Profile
    {
        public TemplateProfile()
        {
            CreateMap<TemplateTheme, TemplateThemeDto>().ReverseMap();

            CreateMap<TemplateAccess, TemplateAccessDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username));

            CreateMap<Template, TemplateDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(tt => tt.Tag.Name)))
                .ForMember(dest => dest.AllowedUserIds, opt => opt.MapFrom(src => src.Accesses.Select(a => a.UserId)));

            CreateMap<Template, TemplateDetailsDto>()
                .IncludeBase<Template, TemplateDto>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Theme, opt => opt.MapFrom(src => src.Theme))
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
                .ForMember(dest => dest.Accesses, opt => opt.MapFrom(src => src.Accesses))
                .ForMember(dest => dest.FormsCount, opt => opt.Ignore())
                .ForMember(dest => dest.LikesCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsLikedByCurrentUser, opt => opt.Ignore());

            CreateMap<Template, TemplateListItemDto>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Theme, opt => opt.MapFrom(src => src.Theme))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(tt => tt.Tag.Name)))
                .ForMember(dest => dest.QuestionsCount, opt => opt.MapFrom(src => src.Questions.Count))
                .ForMember(dest => dest.FormsCount, opt => opt.Ignore())
                .ForMember(dest => dest.LikesCount, opt => opt.Ignore());

            CreateMap<TemplateDto, Template>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Theme, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.Ignore())
                .ForMember(dest => dest.Forms, opt => opt.Ignore())
                .ForMember(dest => dest.Tags, opt => opt.Ignore())
                .ForMember(dest => dest.Accesses, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Likes, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) => {
                    dest.UpdatedAt = DateTime.UtcNow;
                    if (dest.Id == Guid.Empty)
                        dest.CreatedAt = DateTime.UtcNow;
                });
        }
    }

}