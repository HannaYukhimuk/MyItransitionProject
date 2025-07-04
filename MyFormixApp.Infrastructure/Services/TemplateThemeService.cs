using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Templates;
using MyFormixApp.Application.Repositories;
using MyFormixApp.Application.Services;

namespace MyFormixApp.Infrastructure.Services
{
    public class TemplateThemeService : ITemplateThemeService
    {
        private readonly ITemplateThemeRepository _repository;

        public TemplateThemeService(ITemplateThemeRepository repository) => _repository = repository;

        public async Task<IEnumerable<TemplateThemeDto>> GetAllThemesAsync() => 
            (await _repository.GetAllAsync()).Select(MapToDto);

        public async Task<TemplateThemeDto?> GetThemeByIdAsync(int id)
        {
            var theme = await _repository.GetByIdAsync(id);
            return theme == null ? null : MapToDto(theme);
        }

        public async Task<TemplateThemeDto> CreateThemeAsync(TemplateThemeDto dto) => 
            MapToDto(await _repository.CreateAsync(new TemplateTheme { Name = dto.Name }));

        public async Task<TemplateThemeDto?> UpdateThemeAsync(int id, TemplateThemeDto dto)
        {
            var theme = await _repository.GetByIdAsync(id);
            if (theme == null) return null;
            theme.Name = dto.Name;
            await _repository.UpdateAsync(theme);  
            return MapToDto(theme);               
        }

        public async Task<bool> DeleteThemeAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static TemplateThemeDto MapToDto(TemplateTheme theme) => new()
        {
            Id = theme.Id,
            Name = theme.Name
        };
    }
}
