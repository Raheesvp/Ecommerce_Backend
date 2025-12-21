using Application.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public  interface ICartService
    {
        // Get the user's cart
        Task<List<CartItemResponse>> GetCartAsync(int userId);

        // Add a product to the cart
        Task AddToCartAsync(int userId, int productId, int quantity);

        // Update the quantity of an item
        Task UpdateQuantityAsync(int userId, UpdateCartItemRequest request);

        // Remove a specific item
        Task RemoveFromCartAsync(int userId, int productId);

        // Empty the entire cart
        Task ClearCartAsync(int userId);

    }
}
