using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repositories
{
    public  interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<List<Product>> GetAllAsync();
        Task<List<Product>> GetByCategoryAsync(string category);
        Task<List<Product>> SearchAsync(string query);

        Task<List<Product>> GetPaginatedAsync(int pageNumber, int pageSize);

        Task<bool> ExistByNameAsync(string name);

        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);

        Task<int> CountAsync();

        Task<bool> ExistsAsync(int productId);

        Task<List<Product>> GetFeaturedAsync();


    }
}
