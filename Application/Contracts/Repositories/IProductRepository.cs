using Application.DTOs.Product;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repositories
{
    public  interface IProductRepository :IGenericeRepository<Product>
    {
        Task<IReadOnlyList<Product>> GetByCategoryAsync(string category);
        Task<IReadOnlyList<Product>> SearchAsync(string query);

        Task<IReadOnlyList<Product>> GetActivePaginatedAsync(int pageNumber, int pageSize);

        Task<bool> ExistByNameAsync(string name);

        //Task AddAsync(Product product);
        //Task<Product?> GetByIdAsync(int id);
        //Task<List<Product>> GetAllAsync();
        //Task UpdateAsync(Product product);
        //Task DeleteAsync(Product product);

        Task<int> CountAsync();

        Task<int> CountActiveAsync(); // Add this method

        Task<bool> ExistsAsync(int productId);

        Task<IReadOnlyList<Product>> GetFeaturedAsync();

        Task<List<Product>> GetFilteredAsync(ProductFilterRequest productFilter);
        Task<int> GetTotalCountAsync(ProductFilterRequest productFilter);

        Task<List<Product>> GetRelatedByCategoryAsync(string category, int excludeId, int limit);

        Task<IEnumerable<Product>> GetAllArchievedAsync();
        Task<Product?> GetByIdWithDeletedAsync(int id);


    }
}
