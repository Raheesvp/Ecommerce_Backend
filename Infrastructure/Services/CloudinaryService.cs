using Application.Contracts.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic; // Required for List
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CloudinaryService : IFileService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary settings are missing in appsettings.json");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        // 1. Single Upload (You already had this)
        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("Invalid image file");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "products",
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = result.Error?.Message ?? "Unknown Cloudinary error";
                throw new Exception($"Image upload failed: {errorMessage}");
            }

            return result.SecureUrl.ToString();
        }

        // 2. MISSING METHOD: Upload Multiple
        public async Task<List<string>> UploadMultipleAsync(List<IFormFile> files)
        {
            var urls = new List<string>();
            foreach (var file in files)
            {
                // Re-use the single upload logic above
                var url = await UploadAsync(file);
                urls.Add(url);
            }
            return urls;
        }

        // 3. MISSING METHOD: Delete (Cloudinary Version)
        public void Delete(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                // We need to extract the "Public ID" from the URL to delete it.
                // URL: https://res.cloudinary.com/.../v1234/products/my-image.jpg
                // Public ID: products/my-image

                var uri = new Uri(url);
                var fileName = System.IO.Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                var publicId = $"products/{fileName}"; // Match the "Folder" name used in UploadAsync

                var deletionParams = new DeletionParams(publicId);
                _cloudinary.Destroy(deletionParams);
            }
            catch
            {
                // Ignore errors during deletion to prevent app crash
            }
        }
    }
}