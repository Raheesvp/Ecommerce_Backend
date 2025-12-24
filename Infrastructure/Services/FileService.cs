using Application.Contracts.Services;
using CloudinaryDotNet.Actions;
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

        // Add this method to satisfy the Interface
        // CORRECT Delete method for Local Storage (FileService.cs)
        public void Delete(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                // 1. Convert the URL (e.g., "/uploads/img.jpg") to a Windows Path
                // We remove the starting '/' so Path.Combine works correctly
                string filePath = Path.Combine(_environment.WebRootPath, fileUrl.TrimStart('/'));

                // 2. Check if file exists on the Hard Drive and delete it
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log error if needed, or ignore
            }
        }
    }
    }


