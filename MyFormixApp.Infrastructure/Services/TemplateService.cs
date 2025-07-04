using AutoMapper;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Templates;
using MyFormixApp.Domain.DTOs.Questions;
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
            
            var dto = await MapToDetailsDto(template, currentUserId);
            return dto;
        }

        public async Task<IEnumerable<TemplateListItemDto>> GetUserTemplatesAsync(Guid userId) => 
            await MapToListItemDtos(await _templateRepository.GetUserTemplatesAsync(userId));

        public async Task<IEnumerable<TemplateListItemDto>> GetPublicTemplatesAsync() => 
            await MapToListItemDtos(await _templateRepository.GetPublicTemplatesAsync());

        public async Task<(IEnumerable<TemplateListItemDto>, int)> GetPublicTemplatesPagedAsync(int page, int pageSize)
        {
            var templates = await _templateRepository.GetPublicTemplatesPagedAsync(page, pageSize);
            var totalPages = await CalculateTotalPages(pageSize);
            return (await MapToListItemDtos(templates), totalPages);
        }

        public async Task<TemplateDetailsDto> CreateAsync(TemplateDto dto, Guid userId)
        {
            var template = await CreateTemplateEntity(dto, userId);
            await _templateRepository.CreateAsync(template);
            return await GetCreatedTemplate(template.Id, userId);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid? userId)
        {
            if (!await CanDeleteTemplate(id, userId)) return false;
            await _templateRepository.DeleteAsync(id);
            return true;
        }

        public async Task<TemplateDetailsDto?> UpdateAsync(TemplateDto dto, Guid? userId)
        {
            if (!await CanUpdateTemplate(dto, userId)) return null;
            var template = await UpdateTemplateEntity(dto);
            return await GetUpdatedTemplate(template);
        }

        public async Task<IEnumerable<TemplateDto>> SearchByTitleAsync(string title) => 
            await MapTemplates(await _templateRepository.SearchByTitleAsync(title));

        public async Task<IEnumerable<TemplateDto>> SearchByTagAsync(string tagName) => 
            await MapTemplates(await _templateRepository.SearchByTagAsync(tagName));

        public async Task<IEnumerable<TemplateListItemDto>> GetAllTemplatesAsync() => 
            await MapToListItemDtos(await _templateRepository.GetAllTemplatesAsync());

        public async Task<IEnumerable<TemplateDto>> GetPublicTemplatesByTagAsync(string tag) => 
            await MapTemplates(await _templateRepository.GetPublicTemplatesByTagAsync(tag));

        public async Task<IEnumerable<TemplateListItemDto>> GetSharedTemplatesAsync(Guid userId) => 
            await MapToListItemDtos(await _templateRepository.GetSharedTemplatesAsync(userId));

        public async Task<IEnumerable<TemplateListItemDto>> GetUserTemplatesPagedAsync(Guid userId, int page, int pageSize) => 
            await MapToListItemDtos(await _templateRepository.GetUserTemplatesPagedAsync(userId, page, pageSize));

        public async Task<int> GetUserTemplatesCountAsync(Guid userId) => 
            await _templateRepository.GetUserTemplatesCountAsync(userId);

        private async Task<TemplateDetailsDto> MapToDetailsDto(Template template, Guid? currentUserId)
        {
            var dto = _mapper.Map<TemplateDetailsDto>(template);
            dto.FormsCount = await _templateRepository.GetFormsCountAsync(template.Id);
            dto.LikesCount = await _likeRepository.CountAsync(template.Id);
            dto.AllowedUserIds = template.Accesses?.Select(a => a.UserId).ToList() ?? new List<Guid>();
            if (currentUserId.HasValue) dto.IsLikedByCurrentUser = await _likeRepository.ExistsAsync(currentUserId.Value, template.Id);
            return dto;
        }

        private async Task<int> CalculateTotalPages(int pageSize) => 
            (int)Math.Ceiling(await _templateRepository.GetPublicTemplatesCountAsync() / (double)pageSize);

        private async Task<Template> CreateTemplateEntity(TemplateDto dto, Guid userId)
        {
            var template = _mapper.Map<Template>(dto);
            template.UserId = userId;
            template.CreatedAt = template.UpdatedAt = DateTime.UtcNow;
            await ProcessTemplateRelations(dto, template, userId);
            return template;
        }

        private async Task<TemplateDetailsDto> GetCreatedTemplate(Guid id, Guid userId) => 
            await GetByIdAsync(id, userId) ?? throw new InvalidOperationException("Template creation failed");

        private async Task<bool> CanDeleteTemplate(Guid id, Guid? userId)
        {
            var template = await _templateRepository.GetByIdAsync(id);
            return template != null && (!userId.HasValue || template.UserId == userId.Value);
        }

        private async Task<bool> CanUpdateTemplate(TemplateDto dto, Guid? userId)
        {
            if (dto.Id == Guid.Empty) return false;
            var template = await _templateRepository.GetByIdWithDetailsAsync(dto.Id);
            return template != null && (!userId.HasValue || template.UserId == userId.Value);
        }

        private async Task<Template> UpdateTemplateEntity(TemplateDto dto)
        {
            var template = await _templateRepository.GetByIdWithDetailsAsync(dto.Id);
            _mapper.Map(dto, template);
            template.UpdatedAt = DateTime.UtcNow;
            await ProcessTemplateRelations(dto, template, template.UserId);
            await _templateRepository.UpdateAsync(template);
            return template;
        }

        private async Task<TemplateDetailsDto> GetUpdatedTemplate(Template template) => 
            await GetByIdAsync(template.Id, template.UserId);

        private async Task ProcessTemplateRelations(TemplateDto dto, Template template, Guid userId)
        {
            await ProcessTags(dto, template);
            await ProcessAccesses(dto, template, userId);
            ProcessQuestions(dto, template);
        }

        private async Task ProcessTags(TemplateDto dto, Template template)
        {
            template.Tags.Clear();
            if (dto.Tags == null) return;
            foreach (var tagName in dto.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
                template.Tags.Add(new TemplateTag { TagId = (await _tagRepository.GetOrCreateAsync(tagName)).Id });
        }

        private async Task ProcessAccesses(TemplateDto dto, Template template, Guid userId)
        {
            template.Accesses.Clear();
            if (dto.IsPublic || dto.AllowedUserIds == null) return;
            foreach (var allowedUserId in dto.AllowedUserIds.Distinct())
                if (await _userRepository.ExistsAsync(allowedUserId) && allowedUserId != userId)
                    template.Accesses.Add(new TemplateAccess { UserId = allowedUserId, GrantedAt = DateTime.UtcNow });
        }

        private static void ProcessQuestions(TemplateDto dto, Template template)
        {
            template.Questions.Clear();
            if (dto.Questions == null) return;
            foreach (var questionDto in dto.Questions.Where(q => q != null))
                template.Questions.Add(CreateQuestion(questionDto));
        }

        private static Question CreateQuestion(QuestionDto questionDto)
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
            AddQuestionOptions(question, questionDto);
            return question;
        }

        private static void AddQuestionOptions(Question question, QuestionDto questionDto)
        {
            if (questionDto.Options == null) return;
            foreach (var optionDto in questionDto.Options.Where(o => !string.IsNullOrWhiteSpace(o.Text)))
                question.Options.Add(new QuestionOption { Text = optionDto.Text, Position = optionDto.Position });
        }

        private async Task<List<TemplateListItemDto>> MapToListItemDtos(IEnumerable<Template> templates)
        {
            var dtos = new List<TemplateListItemDto>();
            foreach (var template in templates)
                dtos.Add(await MapToListItemDto(template));
            return dtos;
        }

        private async Task<TemplateListItemDto> MapToListItemDto(Template template)
        {
            var dto = _mapper.Map<TemplateListItemDto>(template);
            dto.FormsCount = await _templateRepository.GetFormsCountAsync(template.Id);
            dto.LikesCount = await _templateRepository.GetLikesCountAsync(template.Id);
            dto.QuestionsCount = template.Questions.Count;
            dto.Tags = template.Tags.Select(tt => tt.Tag.Name).ToList();
            return dto;
        }

        private async Task<IEnumerable<TemplateDto>> MapTemplates(IEnumerable<Template> templates) => 
            _mapper.Map<IEnumerable<TemplateDto>>(templates);
    }
}