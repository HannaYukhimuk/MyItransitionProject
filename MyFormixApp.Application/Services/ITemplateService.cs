using MyFormixApp.Domain.DTOs.Templates;

namespace MyFormixApp.Application.Services
{
public interface ITemplateService
    {
        Task<TemplateDetailsDto?> GetByIdAsync(Guid id, Guid? currentUserId = null);
        Task<IEnumerable<TemplateListItemDto>> GetUserTemplatesAsync(Guid userId);
        Task<IEnumerable<TemplateListItemDto>> GetPublicTemplatesAsync();
        Task<TemplateDetailsDto> CreateAsync(TemplateDto dto, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid? userId);
        Task<TemplateDetailsDto?> UpdateAsync(TemplateDto dto, Guid? userId);
        Task<IEnumerable<TemplateDto>> GetPublicTemplatesByTagAsync(string tag);
        Task<IEnumerable<TemplateDto>> SearchByTitleAsync(string title);
        Task<IEnumerable<TemplateDto>> SearchByTagAsync(string tagName);
        Task<IEnumerable<TemplateListItemDto>> GetSharedTemplatesAsync(Guid userId);
        Task<IEnumerable<TemplateListItemDto>> GetAllTemplatesAsync();
        Task<(IEnumerable<TemplateListItemDto> Templates, int TotalPages)> GetPublicTemplatesPagedAsync(int page, int pageSize);
        Task<IEnumerable<TemplateListItemDto>> GetUserTemplatesPagedAsync(Guid userId, int page, int pageSize);
        Task<int> GetUserTemplatesCountAsync(Guid userId);
    }
}