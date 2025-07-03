using MyFormixApp.Domain.DTOs.Likes;

namespace MyFormixApp.Application.Services
{
    public interface ILikeService
    {
        Task<LikeStatusDto> ToggleLikeAsync(LikeDto dto, Guid userId);
        Task<(bool IsSuccess, string Message)> TryToggleLikeAsync(LikeDto dto, Guid userId);
    }
}