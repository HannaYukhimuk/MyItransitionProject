using Microsoft.AspNetCore.Http;

namespace MyFormixApp.Application.Services
{
    public interface ICloudStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task DeleteFileAsync(string fileUrl, string containerName);
    }
}