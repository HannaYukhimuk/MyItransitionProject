using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Likes;
using MyFormixApp.Application.Repositories;
using MyFormixApp.Application.Services;

namespace MyFormixApp.Infrastructure.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly ITemplateRepository _templateRepository;

        public LikeService(ILikeRepository likeRepository, ITemplateRepository templateRepository)
        {
            _likeRepository = likeRepository;
            _templateRepository = templateRepository;
        }

        public async Task<LikeStatusDto> ToggleLikeAsync(LikeDto dto, Guid userId)
        {
            await ValidateTemplateExists(dto.TemplateId);
            var isLiked = await GetCurrentLikeStatus(userId, dto.TemplateId);
            await UpdateLikeStatus(userId, dto.TemplateId, isLiked);
            return await CreateLikeStatusDto(dto.TemplateId, isLiked);
        }

        public async Task<(bool IsSuccess, string Message)> TryToggleLikeAsync(LikeDto dto, Guid userId)
        {
            try
            {
                await ToggleLikeAsync(dto, userId);
                return CreateSuccessResult();
            }
            catch (ArgumentException ex)
            {
                return CreateErrorResult(ex.Message);
            }
            catch (Exception)
            {
                return CreateErrorResult("Ошибка при изменении статуса лайка.");
            }
        }

        private async Task ValidateTemplateExists(Guid templateId)
        {
            if (await _templateRepository.GetByIdAsync(templateId) == null)
                throw new ArgumentException("Template not found");
        }

        private async Task<bool> GetCurrentLikeStatus(Guid userId, Guid templateId) => 
            await _likeRepository.ExistsAsync(userId, templateId);

        private async Task UpdateLikeStatus(Guid userId, Guid templateId, bool isLiked)
        {
            if (isLiked) await _likeRepository.RemoveAsync(userId, templateId);
            else await _likeRepository.AddAsync(CreateLike(userId, templateId));
        }

        private Like CreateLike(Guid userId, Guid templateId) => 
            new Like { UserId = userId, TemplateId = templateId };

        private async Task<LikeStatusDto> CreateLikeStatusDto(Guid templateId, bool isLiked) => 
            new LikeStatusDto
            {
                IsLiked = !isLiked,
                TotalLikes = await _likeRepository.CountAsync(templateId)
            };

        private static (bool, string) CreateSuccessResult() => 
            (true, string.Empty);

        private static (bool, string) CreateErrorResult(string message) => 
            (false, message);
    }
}