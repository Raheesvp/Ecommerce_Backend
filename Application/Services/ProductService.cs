using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Product;
using Domain.Entities;
using Domain.Exceptions;
using System.Linq; // Required for Select()

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IFileService _fileService;

        public ProductService(IProductRepository repository, IFileService fileService)
        {
            _repository = repository;
            _fileService = fileService;
        }

        public async Task<List<ProductResponse>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();
            return products.Select(Map).ToList();
        }

        public async Task<ProductResponse> GetByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            return Map(product);
        }

        public async Task<List<ProductResponse>> GetByCategoryAsync(string category)
        {
            var products = await _repository.GetByCategoryAsync(category);
            return products.Select(Map).ToList();
        }

        public async Task<PagedResponse<ProductResponse>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            var items = await _repository.GetPaginatedAsync(pageNumber, pageSize);
            var total = await _repository.CountAsync();

            return new PagedResponse<ProductResponse>(
                items.Select(Map).ToList(),
                pageNumber,
                pageSize,
                total);
        }

        public async Task<List<ProductSearchResponse>> SearchAsync(string query)
        {
            var products = await _repository.SearchAsync(query);
            return products.Select(p => new ProductSearchResponse
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();
        }

        public async Task CreateAsync(CreateProductRequest request)
        {
            if (await _repository.ExistByNameAsync(request.Name))
                throw new InvalidOperationException("Product already exists");

            // 1. Upload Images to Cloud/Storage first
            var imageUrls = new List<string>();
            if (request.Images != null && request.Images.Any())
            {
                foreach (var file in request.Images)
                {
                    var url = await _fileService.UploadAsync(file);
                    imageUrls.Add(url);
                }
            }

            // 2. Create Product (Without Images in constructor)
            var product = new Product(
                request.Name,
                request.Price,
                request.OriginalPrice,
                request.Stock,
                request.Category,
                request.Description,
                request.Offer,
                request.Rating,
                request.Featured);

            // 3. Add Images Manually using the Helper Method
            foreach (var url in imageUrls)
            {
                product.AddImage(url);
            }

            await _repository.AddAsync(product);
        }

        public async Task UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            // 1. Update Standard Fields
            // Note: I removed imageUrls from here because 'Update' doesn't handle them anymore
            product.Update(
                request.Name,
                request.Price,
                request.Stock,
                request.Category,
                request.Description,
                request.Offer,
                request.Featured);

            // 2. Add New Images (if any are uploaded)
            if (request.Images != null && request.Images.Any())
            {
                foreach (var file in request.Images)
                {
                    var url = await _fileService.UploadAsync(file);
                    // Append the new image to the existing collection
                    product.AddImage(url);
                }
            }

            await _repository.UpdateAsync(product);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            await _repository.DeleteAsync(product);
        }

        // --- MAPPING FIX ---
        private static ProductResponse Map(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Category = p.Category,
            Stock = p.Stock,

            // Fix: p.Images is a List<ProductImage>, but DTO needs List<string>
            // We use .Select(i => i.Url) to extract just the URL string
            Images = p.Images?.Select(i => i.Url).ToList() ?? new List<string>()
        };
    }
}