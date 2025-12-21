using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public  interface IFileService
    {
        Task<string> UploadAsync(IFormFile file);
        Task<List<string>> UploadMultipleAsync(List<IFormFile> files);

        void Delete(string relativePath);
    }
}
