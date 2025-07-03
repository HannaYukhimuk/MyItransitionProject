using Microsoft.AspNetCore.Mvc;
using MyFormixApp.Application.Services;

namespace MyFormixApp.UI.Controllers
{
    [ApiController]
    [Route("api/templates/images")]
    public class TemplateImagesApiController : ControllerBase
    {
        private readonly ICloudStorageService _cloudStorageService;

        public TemplateImagesApiController(ICloudStorageService cloudStorageService) 
            => _cloudStorageService = cloudStorageService;

        [HttpPost("upload-question-image")]
        public async Task<IActionResult> UploadQuestionImage(IFormFile file)
            => file == null || file.Length == 0 
                ? BadRequest("No file uploaded") 
                : Ok(new { imageUrl = await _cloudStorageService.UploadFileAsync(file, "questions") });
    }

}
