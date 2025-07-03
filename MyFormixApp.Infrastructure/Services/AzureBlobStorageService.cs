using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyFormixApp.Application.Services;

namespace MyFormixApp.Infrastructure.Services
{
    public class AzureBlobStorageService : ICloudStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(BlobServiceClient blobServiceClient, ILogger<AzureBlobStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            try
            {
                var containerClient = await GetContainerClientAsync(containerName);
                var blobClient = containerClient.GetBlobClient($"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");

                await using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, true);
                
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Azure Blob Storage");
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileUrl, string containerName)
        {
            try
            {
                var containerClient = await GetContainerClientAsync(containerName);
                await containerClient.DeleteBlobIfExistsAsync(Path.GetFileName(new Uri(fileUrl).LocalPath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Azure Blob Storage");
                throw;
            }
        }

        private async Task<BlobContainerClient> GetContainerClientAsync(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            return containerClient;
        }
    }
}