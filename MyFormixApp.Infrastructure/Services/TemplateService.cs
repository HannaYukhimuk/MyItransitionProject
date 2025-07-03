using AutoMapper;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Templates;
using MyFormixApp.Application.Repositories;
using MyFormixApp.Application.Services;

namespace MyFormixApp.Infrastructure.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IMapper _mapper;

        public TemplateService(
            ITemplateRepository templateRepository,
            ITagRepository tagRepository,
            IUserRepository userRepository,
            ILikeRepository likeRepository,
            IMapper mapper)
        {
            _templateRepository = templateRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
            _likeRepository = likeRepository;
            _mapper = mapper;
        }

        public async Task<TemplateDetailsDto?> GetByIdAsync(Guid id, Guid? currentUserId = null)
        {
            var template = await _templateRepository.GetByIdWithDetailsAsync(id);
            if (template == null) return null;

            var dto = _mapper.Map<TemplateDetailsDto>(template);
            dto.FormsCount = await _templateRepository.GetFormsCountAsync(id);
            dto.LikesCount = await _likeRepository.CountAsync(id);
            dto.AllowedUserIds = template.Accesses?.Select(a => a.UserId).ToList() ?? new List<Guid>();

            if (currentUserId.HasValue)
                dto.IsLikedByCurrentUser = await _likeRepository.ExistsAsync(currentUserId.Value, id);

            return dto;
        }

        public async Task<IEnumerable<TemplateListItemDto>> GetUserTemplatesAsync(Guid userId) => 
            await MapToListItemDtos(await _templateRepository.GetUserTemplatesAsync(userId));

        public async Task<IEnumerable<TemplateListItemDto>> GetPublicTemplatesAsync() => 
            await MapToListItemDtos(await _templateRepository.GetPublicTemplatesAsync());

        public async Task<(IEnumerable<TemplateListItemDto> Templates, int TotalPages)> GetPublicTemplatesPagedAsync(int page, int pageSize)
        {
            var templates = await _templateRepository.GetPublicTemplatesPagedAsync(page, pageSize);
            var totalPages = (int)Math.Ceiling(await _templateRepository.GetPublicTemplatesCountAsync() / (double)pageSize);
            return (await MapToListItemDtos(templates), totalPages);
        }

        public async Task<TemplateDetailsDto> CreateAsync(TemplateDto dto, Guid userId)
        {
            var template = _mapper.Map<Template>(dto);
            template.UserId = userId;
            template.CreatedAt = template.UpdatedAt = DateTime.UtcNow;

            await ProcessTags(dto, template);
            await ProcessAccesses(dto, template, userId);
            ProcessQuestions(dto, template);

            var createdTemplate = await _templateRepository.CreateAsync(template);
            return await GetByIdAsync(createdTemplate.Id, userId) ?? 
                   throw new InvalidOperationException("Template was created but could not be retrieved.");
        }

        public async Task<bool> DeleteAsync(Guid id, Guid? userId)
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null || (userId.HasValue && template.UserId != userId.Value)) return false;
            
            await _templateRepository.DeleteAsync(id);
            return true;
        }

        public async Task<TemplateDetailsDto?> UpdateAsync(TemplateDto dto, Guid? userId)
        {
            if (dto.Id == Guid.Empty) return null;
            
            var template = await _templateRepository.GetByIdWithDetailsAsync(dto.Id);
            if (template == null || (userId.HasValue && template.UserId != userId.Value)) return null;

            _mapper.Map(dto, template);
            template.UpdatedAt = DateTime.UtcNow;

            await ProcessTags(dto, template);
            await ProcessAccesses(dto, template, template.UserId);
            ProcessQuestions(dto, template);

            await _templateRepository.UpdateAsync(template);
            return await GetByIdAsync(template.Id, template.UserId);
        }

        public async Task<IEnumerable<TemplateDto>> SearchByTitleAsync(string title) => 
            _mapper.Map<IEnumerable<TemplateDto>>(await _templateRepository.SearchByTitleAsync(title));

        public async Task<IEnumerable<TemplateDto>> SearchByTagAsync(string tagName) => 
            _mapper.Map<IEnumerable<TemplateDto>>(await _templateRepository.SearchByTagAsync(tagName));

        public async Task<IEnumerable<TemplateListItemDto>> GetAllTemplatesAsync() => 
            await MapToListItemDtos(await _templateRepository.GetAllTemplatesAsync());

        public async Task<IEnumerable<TemplateDto>> GetPublicTemplatesByTagAsync(string tag) => 
            _mapper.Map<IEnumerable<TemplateDto>>(await _templateRepository.GetPublicTemplatesByTagAsync(tag));

        public async Task<IEnumerable<TemplateListItemDto>> GetSharedTemplatesAsync(Guid userId) => 
            await MapToListItemDtos(await _templateRepository.GetSharedTemplatesAsync(userId));

        public async Task<IEnumerable<TemplateListItemDto>> GetUserTemplatesPagedAsync(Guid userId, int page, int pageSize) => 
            await MapToListItemDtos(await _templateRepository.GetUserTemplatesPagedAsync(userId, page, pageSize));

        public async Task<int> GetUserTemplatesCountAsync(Guid userId) => 
            await _templateRepository.GetUserTemplatesCountAsync(userId);

        private async Task ProcessTags(TemplateDto dto, Template template)
        {
            template.Tags.Clear();
            if (dto.Tags == null) return;
            
            foreach (var tagName in dto.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var tag = await _tagRepository.GetOrCreateAsync(tagName);
                template.Tags.Add(new TemplateTag { TagId = tag.Id });
            }
        }

        private async Task ProcessAccesses(TemplateDto dto, Template template, Guid userId)
        {
            template.Accesses.Clear();
            if (dto.IsPublic || dto.AllowedUserIds == null) return;
            
            foreach (var allowedUserId in dto.AllowedUserIds.Distinct())
            {
                if (await _userRepository.ExistsAsync(allowedUserId) && allowedUserId != userId)
                {
                    template.Accesses.Add(new TemplateAccess 
                    { 
                        UserId = allowedUserId, 
                        GrantedAt = DateTime.UtcNow 
                    });
                }
            }
        }

        private static void ProcessQuestions(TemplateDto dto, Template template)
        {
            template.Questions.Clear();
            if (dto.Questions == null) return;
            
            foreach (var questionDto in dto.Questions.Where(q => q != null))
            {
                var question = new Question
                {
                    Type = questionDto.Type ?? "text",
                    Title = questionDto.Title ?? "Untitled Question",
                    Description = questionDto.Description,
                    IsRequired = questionDto.IsRequired,
                    Position = questionDto.Position,
                    ShowInTable = questionDto.ShowInTable,
                    CreatedAt = DateTime.UtcNow
                };

                if (questionDto.Options != null)
                {
                    foreach (var optionDto in questionDto.Options.Where(o => !string.IsNullOrWhiteSpace(o.Text)))
                    {
                        question.Options.Add(new QuestionOption 
                        { 
                            Text = optionDto.Text, 
                            Position = optionDto.Position 
                        });
                    }
                }

                template.Questions.Add(question);
            }
        }

        private async Task<List<TemplateListItemDto>> MapToListItemDtos(IEnumerable<Template> templates)
        {
            var dtos = new List<TemplateListItemDto>();
            foreach (var template in templates)
            {
                var dto = _mapper.Map<TemplateListItemDto>(template);
                dto.FormsCount = await _templateRepository.GetFormsCountAsync(template.Id);
                dto.LikesCount = await _templateRepository.GetLikesCountAsync(template.Id);
                dto.QuestionsCount = template.Questions.Count;
                dto.Tags = template.Tags.Select(tt => tt.Tag.Name).ToList();
                dtos.Add(dto);
            }
            return dtos;
        }
        
    }
}