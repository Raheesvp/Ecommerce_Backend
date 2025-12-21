using Application.Contracts.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public  class FileService :IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return string.Empty;

            
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");

        
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            
            return $"/uploads/products/{uniqueFileName}";
        }

        public async Task<List<string>> UploadMultipleAsync(List<IFormFile> files)
        {
            var paths = new List<string>();
            foreach (var file in files)
            {
                var path = await UploadAsync(file);
                if (!string.IsNullOrEmpty(path)) paths.Add(path);
            }
            return paths;
        }

        public void Delete(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return;

            var fullPath = Path.Combine(
                _environment.WebRootPath,
                relativePath.TrimStart('/')
            );

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}

