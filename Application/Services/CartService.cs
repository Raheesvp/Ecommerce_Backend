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
        private readonly IProductRepository _productRepository;

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

                Image = c.Product.Images?.FirstOrDefault()?.Url,
                Stock = c.Product.Stock
            }).ToList();
        }

        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            // 1. Get Product & Validate
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new NotFoundException("Product Not Found");

          
            if (product.Stock < quantity)
            {
                throw new InvalidOperationException($"Not Enough Stock. Only {product.Stock} left");
            }

            var existingItem = await _cartRepository.GetCartItemAsync(userId, productId);

            if (existingItem != null)
            {
                


                if (product.Stock < (existingItem.Quantity + quantity))
                {
                    throw new InvalidOperationException($"Cannot Add More. You have {existingItem.Quantity} in cart and we only have {product.Stock} in stock");
                }

                
                existingItem.Quantity += quantity;
                await _cartRepository.UpdateItemAsync(existingItem);
            }
            else
            {
             
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
                
                await _cartRepository.RemoveItemAsync(cartItem);
            }
            else
            {
                
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null) throw new NotFoundException("Product Not Found");
            
                if (product.Stock < request.Quantity)
                {
                    throw new InvalidOperationException($"Cannot update. Only {product.Stock} items are in stock.");
                }
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
