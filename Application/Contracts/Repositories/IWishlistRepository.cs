using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repositories
{
    public  interface IWishlistRepository
    {
        Task<bool> ExistsAsync(int userId, int productId);
        Task AddAsync(int userId, int productId);
        Task RemoveAsync(int userId, int productId);
        Task<List<Product>> GetWishlistByUserIdAsync(int userId);
        Task ClearAsync(int userId);
    }
}
