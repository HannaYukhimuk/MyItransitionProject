using AutoMapper;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Answers;
using MyFormixApp.Domain.DTOs.Questions;

namespace MyFormixApp.Application.Mapping
{
    public class AnswerProfile : Profile
{
    public AnswerProfile()
    {
        CreateMap<QuestionOption, QuestionOptionDto>();
        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        CreateMap<Answer, AnswerDetailsDto>()
            .ForMember(dest => dest.QuestionTitle, opt => opt.MapFrom(src => src.Question.Title))
            .ForMember(dest => dest.QuestionDescription, opt => opt.MapFrom(src => src.Question.Description))
            .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.Question.Type))
            .ForMember(dest => dest.NumberValue, opt => opt.MapFrom(src => src.ValueNumber))
            .ForMember(dest => dest.BoolValue, opt => opt.MapFrom(src => src.ValueBool))
            .ForMember(dest => dest.Options, opt => opt.Ignore())
            .ForMember(dest => dest.TextValue, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                dest.Options = src.Question?.Options != null
                    ? src.Question.Options.OrderBy(o => o.Position).Select(o => o.Text).ToList()
                    : new List<string>();

                if (src.Question?.Type == "radio" || src.Question?.Type == "select")
                {
                    var match = src.Question?.Options?
                        .FirstOrDefault(o => o.Id.ToString().Equals(src.ValueText, StringComparison.OrdinalIgnoreCase));
                    dest.TextValue = match?.Text ?? src.ValueText;
                }
                else
                {
                    dest.TextValue = src.ValueText;
                }
            });

    }
}

}