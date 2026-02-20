using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Category;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        private readonly IProductRepository _productRepository;

        private readonly IUnitOfWork _unitOfWork;

        // FIX: Added missing constructor
        public CategoryService(ICategoryRepository categoryRepository,IProductRepository productRepository,IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            
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

        public async Task<CategoryResponse> UpdateCategoryAsync(int id, string name)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) throw new Exception("Category not found");

            category.Name = name;
            await _categoryRepository.UpdateAsync(category);
            return new CategoryResponse { Id = category.Id, Name = category.Name };
        }


        public async Task DeleteCategoryAsync(int id)
        {
            await _unitOfWork.ExecuteAsync(async () =>
            {
                var category = await _categoryRepository.GetByIdAsync(id)
                    ?? throw new NotFoundException("Category Not Found");

                var products = await _productRepository.GetByCategoryAsync(category.Name);

                if (products.Any())
                {
                    foreach (var product in products)
                    {
                        product.IsActive = false;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                await _categoryRepository.DeleteAsync(category);
            });
        }

    }
}