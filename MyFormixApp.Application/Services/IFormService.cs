using MyFormixApp.Domain.DTOs.Forms;
using MyFormixApp.Domain.DTOs.Templates;

namespace MyFormixApp.Application.Services
{
    public interface IFormService
    {
        Task<FormDetailsDto?> GetByIdAsync(Guid id, Guid currentUserId);
        Task<IEnumerable<FormDetailsDto>> GetByUserAsync(Guid userId);
        Task<bool> UpdateAsync(FormDto dto, Guid userId, bool isAdmin = false);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<bool> CreateResponseAsync(Guid templateId, Guid userId, Dictionary<Guid, string> responses);
        Task<IEnumerable<FormDetailsDto>> GetAllFormsAsync();
        Task<IEnumerable<FormDetailsDto>> GetByTemplateAsync(Guid templateId, Guid currentUserId);
        Task<TemplateStatisticsDto> GetTemplateStatisticsAsync(Guid templateId, Guid currentUserId);
        Task<FormDetailsDto?> GetByUserAndTemplateAsync(Guid userId, Guid templateId);
    }
}