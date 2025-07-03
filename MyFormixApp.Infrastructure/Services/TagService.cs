using AutoMapper;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Domain.DTOs.Tags;
using MyFormixApp.Application.Repositories;
using MyFormixApp.Application.Services;

namespace MyFormixApp.Infrastructure.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;

        public TagService(ITagRepository tagRepository, IMapper mapper)
        {
            _tagRepository = tagRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count) => 
            await MapToDtos(await _tagRepository.GetPopularTagsAsync(count));

        public async Task<IEnumerable<TagCloudItemDto>> GetTagCloudAsync(int maxTags = 50) => 
            (await _tagRepository.GetPopularTagsAsync(maxTags))
                .Select(t => new TagCloudItemDto { Name = t.Name, Count = t.Templates.Count });

        public async Task<IEnumerable<TagDto>> SearchTagsAsync(string term, int limit) => 
            await MapToDtos(await _tagRepository.SearchTagsAsync(term, limit));

        private async Task<List<TagDto>> MapToDtos(IEnumerable<Tag> tags)
        {
            var dtos = new List<TagDto>();
            foreach (var tag in tags)
            {
                var dto = _mapper.Map<TagDto>(tag);
                dto.UsageCount = await _tagRepository.GetUsageCountAsync(tag.Id);
                dtos.Add(dto);
            }
            return dtos;
        }
    }
}
