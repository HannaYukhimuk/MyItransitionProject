using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using MyFormixApp.UI.ViewModels;

namespace MyFormixApp.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly ITagService _tagService;

        public HomeController(ITemplateService templateService, ITagService tagService)
        {
            _templateService = templateService;
            _tagService = tagService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 6;

            var (templates, totalPages) = await _templateService.GetPublicTemplatesPagedAsync(page, pageSize);

            var topTemplates = (await _templateService.GetPublicTemplatesAsync())
                .OrderByDescending(t => t.LikesCount)
                .Take(5)
                .ToList();

            var popularTags = (await _tagService.GetTagCloudAsync(10)).ToList();

            var model = new HomeViewModel
            {
                RecentTemplates = templates.ToList(),
                TopTemplates = topTemplates,
                PopularTags = popularTags,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }
    }
}
