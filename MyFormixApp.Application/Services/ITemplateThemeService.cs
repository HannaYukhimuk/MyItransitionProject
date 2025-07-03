using MyFormixApp.Domain.DTOs.Templates;

namespace MyFormixApp.Application.Services
{
    public interface ITemplateThemeService
    {
        Task<IEnumerable<TemplateThemeDto>> GetAllThemesAsync();
        Task<TemplateThemeDto?> GetThemeByIdAsync(int id);
        Task<TemplateThemeDto> CreateThemeAsync(TemplateThemeDto dto);
        Task<TemplateThemeDto?> UpdateThemeAsync(int id, TemplateThemeDto dto);
        Task<bool> DeleteThemeAsync(int id);
    }
}