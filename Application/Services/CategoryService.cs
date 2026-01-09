using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Category;
using Domain.Entities;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        // FIX: Added missing constructor
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryResponse>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new CategoryResponse { Id = c.Id, Name = c.Name }).ToList();
        }

        public async Task<CategoryResponse> CreateCategoryAsync(string name)
        {
            var existing = await _categoryRepository.GetByNameAsync(name);
            if (existing != null) return new CategoryResponse { Id = existing.Id, Name = existing.Name };

            var category = new CategoryEntity { Name = name };
            await _categoryRepository.AddAsync(category);
            return new CategoryResponse { Id = category.Id, Name = category.Name };
        }

        // FIX: Added missing Update logic
        public async Task<CategoryResponse> UpdateCategoryAsync(int id, string name)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) throw new Exception("Category not found");

            category.Name = name;
            await _categoryRepository.UpdateAsync(category);
            return new CategoryResponse { Id = category.Id, Name = category.Name };
        }

        // FIX: Added missing Delete logic
        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                // Real-world check: You might want to check if products exist in this category first
                await _categoryRepository.DeleteAsync(category);
            }
        }
    }
}