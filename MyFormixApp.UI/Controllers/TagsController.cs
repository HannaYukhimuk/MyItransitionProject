using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace MyFormixApp.UI.Controllers
{
    [AllowAnonymous]
    public class TagsController : Controller
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<IActionResult> Autocomplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new string[0]);
            }
            var tags = await _tagService.SearchTagsAsync(term, 10);
            return Json(tags.Select(t => t.Name));
        }

        [HttpGet]
        public async Task<IActionResult> Popular(int count = 10)
        {
            var tags = await _tagService.GetPopularTagsAsync(count);
            return PartialView("_PopularTags", tags);
        }
    }
}