using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Product;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _unitOfWork;


        public ProductService(IProductRepository repository, IFileService fileService,IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _fileService = fileService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductResponse>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();

            return products
         //.Where(p => p.IsActive == true) 
         .Select(Map)
         .ToList();
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
            var items = await _repository.GetActivePaginatedAsync(pageNumber, pageSize);
            var total = await _repository.CountActiveAsync();

            var response = items.Select(Map).ToList();

            return new PagedResponse<ProductResponse>(
              
                response,
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
                Name = p.Name,
                Price = p.Price,
                Category = p.Category,
                Description = p.Description,
            
                Image = p.Images?.FirstOrDefault()?.Url

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
            await _unitOfWork.CommitTransactionAsync();
        }

        public async Task UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            if (request.Name != product.Name && await _repository.ExistByNameAsync(request.Name))
            {
                throw new InvalidOperationException($"Product with name '{request.Name}' already exists.");
            }


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
            await _unitOfWork.CommitTransactionAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            product.IsActive = false; // Just hide it
            await _repository.UpdateAsync(product);

            await _unitOfWork.CommitTransactionAsync();
        }
        

        //getting the filtered products

        public async Task<Object> GetProductsAsync(ProductFilterRequest productFilter)
        {
            var products = await _repository.GetFilteredAsync(productFilter);
            var total = await _repository.GetTotalCountAsync(productFilter);

            return new
            {
                TotalCount = total,
                Items = products.Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Rating = p.Rating
                })
            };
        }

        private static ProductResponse Map(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description ?? string.Empty,
            Rating = p.Rating,
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

        //filtered products

        public async Task<PagedResponse<ProductResponse>> GetFilteredAsync(
        ProductFilterRequest filter)
        {
            var products = await _repository.GetFilteredAsync(filter);
            var total = await _repository.GetTotalCountAsync(filter);

            return new PagedResponse<ProductResponse>
            {
                TotalCount = total,
                PageNumber = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = filter.PageSize == 0
        ? 0
        : (int)Math.Ceiling(total / (double)filter.PageSize),
                Items = products.Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Rating = p.Rating,
                    Description = p.Description,
                    Stock = p.Stock,
                     CreatedAt = p.CreatedAt,
                    Category = p.Category,
                     Images = p.Images.Any()
            ? p.Images.Select(img => img.Url).ToList()
            : new List<string> { p.ImageUrl }
                }).ToList()
            };
        }

        //get related products

        public async Task<List<ProductResponse>> GetRelatedProductsAsync(int productId)
        {
            var product = await _repository.GetByIdAsync(productId);
            if (product == null) return new List<ProductResponse>();

            var related = await _repository.GetRelatedByCategoryAsync(product.Category, productId, 10);

            return related.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category,
                Description = p.Description,
                Stock = p.Stock,
                Image = p.Images?.FirstOrDefault()?.Url
            }).ToList();
        }
    }
}