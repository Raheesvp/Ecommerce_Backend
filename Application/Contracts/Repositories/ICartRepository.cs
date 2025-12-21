using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repositories
{
    public  interface ICartRepository
    {
        // Get all items in the user's cart
        Task<List<CartEntity>> GetCartByUserIdAsync(int userId);

        // Get a specific item (to check if it exists or update quantity)
        Task<CartEntity?> GetCartItemAsync(int userId, int productId);

        // Add a new item
        Task AddItemAsync(CartEntity cartItem);

        // Update an existing item (e.g., change quantity)
        Task UpdateItemAsync(CartEntity cartItem);

        // Remove a single item
        Task RemoveItemAsync(CartEntity cartItem);

        // Delete everything in the cart
        Task ClearCartAsync(int userId);
    }
}
