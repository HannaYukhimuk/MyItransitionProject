using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;

namespace MyFormixApp.UI.Controllers
{
    [AllowAnonymous]
    public class TemplateSearchController : Controller
    {
        private readonly ITemplateService _templateService;

        public TemplateSearchController(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchByTitle(string title)
        {
            var results = await _templateService.SearchByTitleAsync(title);
            return PartialView("_TemplateListPartial", results);
        }

        [HttpGet]
        public async Task<IActionResult> SearchByTag(string tag)
        {
            var results = await _templateService.SearchByTagAsync(tag);
            return PartialView("_TemplateListPartial", results);
        }
    }
}