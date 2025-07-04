using MyFormixApp.Domain.DTOs.Templates;
using MyFormixApp.Domain.DTOs.Tags;
using System.Collections.Generic;

namespace MyFormixApp.UI.ViewModels
{
    public class HomeViewModel
{
    public List<TemplateListItemDto> RecentTemplates { get; set; } = new();
        public List<TemplateListItemDto> TopTemplates { get; set; } = new();
        public List<TagCloudItemDto> PopularTags { get; set; } = new();

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

}
