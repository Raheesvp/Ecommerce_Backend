using Application.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public  interface ICategoryService
    {
        Task<List<CategoryResponse>> GetAllCategoriesAsync();

        Task<CategoryResponse> CreateCategoryAsync(string name);

        Task<CategoryResponse> UpdateCategoryAsync(int id, string name); // Add this
        Task DeleteCategoryAsync(int id);


    }
}
