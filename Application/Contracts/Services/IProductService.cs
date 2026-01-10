using Application.DTOs.Category;
using Application.DTOs.Product;

namespace Application.Contracts.Services
{
    public interface IProductService
    {
        Task<List<ProductResponse>> GetAllAsync();
        Task<ProductResponse> GetByIdAsync(int id);
        Task<List<ProductResponse>> GetByCategoryAsync(string category);
        Task<PagedResponse<ProductResponse>> GetPaginatedAsync(int pageNumber, int pageSize);
        Task<List<ProductSearchResponse>> SearchAsync(string query);

        Task CreateAsync(CreateProductRequest request);
        Task UpdateAsync(int id, UpdateProductRequest request);
        Task DeleteAsync(int id);

        Task<List<ProductResponse>> GetFeaturedProductAsync();


        Task<PagedResponse<ProductResponse>> GetFilteredAsync(ProductFilterRequest productFilter);

        Task<List<ProductResponse>> GetRelatedProductsAsync(int productId);


        Task<List<ProductCategoryResponse>> GetProductsByCategoryIdAsync(int categoryId);



        //Task<bool> ExistByNameAsync(string name);

    }
}
