using MyFormixApp.Domain.DTOs.Forms;
using MyFormixApp.Application.Models;
using MyFormixApp.Domain.DTOs.Templates;
using Microsoft.AspNetCore.Http;
using MyFormixApp.Application.Results.Forms;

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
        Task<FormResult> ProcessFormResponseAsync(Guid templateId, Guid userId, IFormCollection formCollection);
        Task<ServiceResult<TemplateFormsView>> GetTemplateFormsAsync(Guid templateId, Guid userId);
        Task<ServiceResult<FormDetailsDto>> GetFormDetailsAsync(Guid id, Guid userId);
        Task<FormOperationResult> UpdateFormAsync(FormDto dto, Guid userId, bool isAdmin);
        Task<OperationResult> DeleteFormAsync(Guid id, Guid userId);
        Task<IEnumerable<FormDetailsDto>> GetUserFormsAsync(Guid userId);
    }
}