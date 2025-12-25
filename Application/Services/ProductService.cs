using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Product;
using Domain.Entities;
using Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
            var name = request.Name?.Trim();
            var category = request.Category?.Trim();
            var description = request.Description?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Product name is required");

            if (await _repository.ExistByNameAsync(name))
                throw new InvalidOperationException("Product already exists");

            var imageUrls = new List<string>();
            if (request.Images != null && request.Images.Any())
            {
                foreach (var file in request.Images)
                {
                    var url = await _fileService.UploadAsync(file);
                    imageUrls.Add(url);
                }
            }

            // Determine Main Image (First uploaded image, or empty)
            string mainImageUrl = imageUrls.FirstOrDefault() ?? string.Empty;

            // --- CHANGE 2: Pass Main Image to Constructor ---
            var product = new Product(
                request.Name,
                request.Price,
                request.OriginalPrice,
                request.Stock,
                request.Category,
                request.Description,
                request.Offer,
                request.Rating,
                request.Featured,
                mainImageUrl
            );

            if (request.OriginalPrice <= request.Price)
                throw new ValidationException(
                    "OriginalPrice must be greater than Price"
                );


            // Add all images to the gallery collection
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

            // --- CHANGE 3: Update Call ---
            // We pass 'null' for imageUrl here because we aren't changing the main image explicitly via text
            // The AddImage logic below will handle it if the product was empty.
            product.Update(
                request.Name,
                request.Price,
                request.Stock,
                request.Category,
                request.Description,
                null, 
                request.Offer,
                request.Featured);

            // Add New Images (if any are uploaded)
            if (request.Images != null && request.Images.Any())
            {
                foreach (var file in request.Images)
                {
                    var url = await _fileService.UploadAsync(file);
                    // This adds to gallery AND sets Main Image if it was empty
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

        private static ProductResponse Map(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Category = p.Category,
            Stock = p.Stock,
            Images = p.Images?.Select(i => i.Url).ToList() ?? new List<string>(),
        };

        public async Task<List<ProductResponse>> GetFeaturedProductAsync()
        {
            var products = await _repository.GetFeaturedAsync();
            return products.Select(Map).ToList();
        }
    }
}