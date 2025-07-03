using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Comments;
using MyFormixApp.Application.Repositories;
using MyFormixApp.Application.Services;

namespace MyFormixApp.Infrastructure.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ITemplateRepository _templateRepository;

        public CommentService(ICommentRepository commentRepository, ITemplateRepository templateRepository)
        {
            _commentRepository = commentRepository;
            _templateRepository = templateRepository;
        }

        public async Task<CommentDetailsDto?> GetByIdAsync(Guid id) =>
            MapToDto(await _commentRepository.GetByIdAsync(id));

        public async Task<IEnumerable<CommentDetailsDto>> GetByTemplateAsync(Guid templateId) =>
            BuildCommentTree(await _commentRepository.GetByTemplateWithRepliesAsync(templateId));

        public async Task<CommentDetailsDto> CreateAsync(CommentDto dto, Guid userId)
        {
            await ValidateTemplateAndParentComment(dto);

            var comment = new Comment
            {
                TemplateId = dto.TemplateId,
                UserId = userId,
                Text = dto.Text,
                ParentCommentId = dto.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            return MapToDto(await _commentRepository.CreateAsync(comment));
        }

        public async Task<CommentDetailsDto?> UpdateAsync(Guid id, CommentDto dto, Guid userId)
        {
            var comment = await GetCommentIfBelongsToUser(id, userId);
            if (comment == null) return null;

            comment.Text = dto.Text;
            await _commentRepository.UpdateAsync(comment);
            return MapToDto(comment);
        }


        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var comment = await GetCommentIfBelongsToUser(id, userId);
            if (comment == null) return false;

            await _commentRepository.DeleteAsync(id);
            return true;
        }

        private async Task ValidateTemplateAndParentComment(CommentDto dto)
        {
            if (await _templateRepository.GetByIdAsync(dto.TemplateId) == null)
                throw new ArgumentException("Template not found");

            if (dto.ParentCommentId.HasValue && !await _commentRepository.ExistsAsync(dto.ParentCommentId.Value))
                throw new ArgumentException("Parent comment not found");
        }

        private async Task<Comment?> GetCommentIfBelongsToUser(Guid id, Guid userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            return comment?.UserId == userId ? comment : null;
        }

        private List<CommentDetailsDto> BuildCommentTree(IEnumerable<Comment> comments)
        {
            var commentDict = comments.ToDictionary(c => c.Id, MapToDto);

            comments.Where(c => c.ParentCommentId != null)
                .ToList()
                .ForEach(c =>
                {
                    if (commentDict.TryGetValue(c.ParentCommentId!.Value, out var parent))
                        parent.Replies.Add(commentDict[c.Id]);
                });

            return commentDict.Values.Where(c => c.ParentCommentId == null).ToList();
        }

        private static CommentDetailsDto MapToDto(Comment comment) => new()
        {
            Id = comment.Id,
            TemplateId = comment.TemplateId,
            UserId = comment.UserId,
            UserName = comment.User?.Username ?? "Deleted User",
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            ParentCommentId = comment.ParentCommentId,
            Replies = new List<CommentDetailsDto>()
        };
        

        public async Task<(bool IsSuccess, string Message)> TryCreateAsync(CommentDto dto, Guid userId)
        {
            try
            {
                await CreateAsync(dto, userId);
                return (true, "Comment successfully added");
            }
            catch (ArgumentException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception)
            {
                return (false, "Error adding comment");
            }
        }
    }
}