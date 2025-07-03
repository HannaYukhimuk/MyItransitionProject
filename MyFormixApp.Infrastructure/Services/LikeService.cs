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
            if (await _templateRepository.GetByIdAsync(dto.TemplateId) == null)
                throw new ArgumentException("Template not found");

            var isLiked = await _likeRepository.ExistsAsync(userId, dto.TemplateId);

            if (isLiked)
                await _likeRepository.RemoveAsync(userId, dto.TemplateId);
            else
                await _likeRepository.AddAsync(new Like { UserId = userId, TemplateId = dto.TemplateId });

            return new LikeStatusDto
            {
                IsLiked = !isLiked,
                TotalLikes = await _likeRepository.CountAsync(dto.TemplateId)
            };
        }
        
        public async Task<(bool IsSuccess, string Message)> TryToggleLikeAsync(LikeDto dto, Guid userId)
        {
            try
            {
                await ToggleLikeAsync(dto, userId);
                return (true, string.Empty);
            }
            catch (ArgumentException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception)
            {
                return (false, "Ошибка при изменении статуса лайка.");
            }
        }

    }
}