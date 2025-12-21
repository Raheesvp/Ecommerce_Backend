using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Cart;
using Domain.Entities;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository; // To validate product existence

        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<List<CartItemResponse>> GetCartAsync(int userId)
        {
            var items = await _cartRepository.GetCartByUserIdAsync(userId);

            return items.Select(c => new CartItemResponse
            {
                ProductId = c.ProductId,
                Name = c.Product.Name,
                Price = c.Product.Price,
                Quantity = c.Quantity,
                TotalPrice = c.Product.Price * c.Quantity,

                // FIX: Safely grab the first image URL
                Image = c.Product.Images?.FirstOrDefault()?.Url
            }).ToList();
        }

        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            // 1. Validate Product Exists
            if (!await _productRepository.ExistsAsync(productId))
                throw new NotFoundException("Product not found");

            // 2. Check if item is already in cart
            var existingItem = await _cartRepository.GetCartItemAsync(userId, productId);

            if (existingItem != null)
            {
                // Logic: If exists, increase quantity
                existingItem.IncreaseQuantity(quantity);
                await _cartRepository.UpdateItemAsync(existingItem);
            }
            else
            {
                // Logic: If new, create item
                var newItem = new CartEntity(userId, productId, quantity);
                await _cartRepository.AddItemAsync(newItem);
            }
        }

        public async Task UpdateQuantityAsync(int userId, UpdateCartItemRequest request)
        {
            var cartItem = await _cartRepository.GetCartItemAsync(userId, request.ProductId);

            if (cartItem == null)
                throw new NotFoundException("Item not found in cart");

            if (request.Quantity <= 0)
            {
                // Remove if quantity is zero
                await _cartRepository.RemoveItemAsync(cartItem);
            }
            else
            {
                cartItem.UpdateQuantity(request.Quantity);
                await _cartRepository.UpdateItemAsync(cartItem);
            }
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            var cartItem = await _cartRepository.GetCartItemAsync(userId, productId);
            if (cartItem == null) throw new NotFoundException("Cart item not found");

            await _cartRepository.RemoveItemAsync(cartItem);
        }

        public async Task ClearCartAsync(int userId)
        {
            await _cartRepository.ClearCartAsync(userId);
        }
    }
}
