using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Category;
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
        private readonly ICartRepository _cartRepository;
        private readonly ICategoryRepository _categoryRepository;


        public ProductService(IProductRepository repository, IFileService fileService,IUnitOfWork unitOfWork,ICategoryRepository categoryRepository,ICartRepository cartRepository )
        {
            _repository = repository;
            _fileService = fileService;
            _unitOfWork = unitOfWork;
            _categoryRepository = categoryRepository;
            _cartRepository = cartRepository;

        }

        public async Task<List<ProductResponse>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();

            return products
         .Where(p => p.IsActive)
         .Select(Map)
         .ToList();
        }

        public async Task<ProductResponse> GetByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

               return Map(product);
        }

        //category wise product 

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
            // 1. Basic Validations
            var name = request.Name?.Trim();
            var categoryName = request.Category.ToString().Trim();
            var description = request.Description?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Product name is required");

            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ValidationException("Category name is required");

            if (await _repository.ExistByNameAsync(name))
                throw new InvalidOperationException("Product already exists");

            if (request.OriginalPrice <= request.Price)
                throw new ValidationException("OriginalPrice must be greater than Price");

            // 2. Resolve Category (String name to Integer ID)
            // We look up the category. If it doesn't exist, we create a new one.
            var categoryEntity = await _categoryRepository.GetByNameAsync(categoryName);
            int finalCategoryId;

            if (categoryEntity == null)
            {
                var newCategory = new CategoryEntity { Name = categoryName };
                var createdCategory = await _categoryRepository.AddAsync(newCategory);
                finalCategoryId = createdCategory.Id;
            }
            else
            {
                finalCategoryId = categoryEntity.Id;
            }

            // 3. Handle File Uploads
            var imageUrls = new List<string>();
            if (request.Images != null && request.Images.Any())
            {
                foreach (var file in request.Images)
                {
                    var url = await _fileService.UploadAsync(file);
                    imageUrls.Add(url);
                }
            }

            // Determine Main Image
            string mainImageUrl = imageUrls.FirstOrDefault() ?? string.Empty;

            // 4. Initialize Product with CategoryId (int)
            // Note: I changed request.Category to finalCategoryId
            var product = new Product(
                request.Name,
                request.Price,
                request.OriginalPrice,
                request.Stock,
                categoryName,
                request.Description,
                request.Offer,
                request.Rating,
                request.Featured,
                mainImageUrl
            );

            // 5. Add Gallery Images
            foreach (var url in imageUrls)
            {
                product.AddImage(url);
            }

            // 6. Save and Commit
            await _repository.AddAsync(product);
            await _unitOfWork.CommitTransactionAsync();
        }

        public async Task UpdateAsync(int id, UpdateProductRequest request)
        {
            // 1. Fetch the product - Ensure it is NOT AsNoTracking in the repository
            var product = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            // 2. Handle Images
            string? newMainImageUrl = product.ImageUrl;

            if (request.Images != null && request.Images.Any())
            {
                foreach (var file in request.Images)
                {
                    var uploadedUrl = await _fileService.UploadAsync(file);
                    product.AddImage(uploadedUrl);
                    newMainImageUrl = uploadedUrl;
                }
            }

            // 3. Update the Entity Properties
            // Check if request.Stock actually has a value from the frontend
            product.Update(
                request.Name,
                request.Price,
                request.Stock,
                request.Category,
                request.Description,
                newMainImageUrl,
                request.Offer,
                request.Featured);

            // 4. SOLUTION: Mark as Modified
            // This tells EF Core to generate the SQL: UPDATE Products SET Stock = @p0...
            await _repository.UpdateAsync(product);

            // 5. Commit the Transaction
            // If this method doesn't call _context.SaveChangesAsync(), nothing happens in DB
            await _unitOfWork.CommitTransactionAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            product.IsActive = false;
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
            Image = p.Images != null && p.Images.Any()
            ? p.Images.First().Url
            : p.ImageUrl
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
        //get archieved products

        public async Task<List<ProductResponse>> GetArchieveProductAsync()
        {
            var archivedProducts = await _repository.GetAllArchievedAsync();

            return archivedProducts.Select(Map).ToList();
        }

        //restore prodcuts

        public async Task RestoreAsync(int id)
        {
            var product = await _repository.GetByIdWithDeletedAsync(id) 
                ?? throw new NotFoundException("Product not found in archieve");

            product.IsActive = true;
            await _repository.UpdateAsync(product);
            await _unitOfWork.CommitTransactionAsync();
        }
    }
}