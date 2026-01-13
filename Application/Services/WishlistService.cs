using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Wishlist;
using Domain.Enums;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
   public class  WishlistService: IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;

        private readonly IProductRepository _productRepository;

        public WishlistService(IWishlistRepository wishlistRepository,IProductRepository productRepository)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
        }

        public async Task<WishlistActionEnum> ToggleWishlistAsync(int userId, int productId)
        {
            bool productExists = await _productRepository.ExistsAsync(productId);

            if (!productExists)
            {
                // Throw an error if the ID is bad. This prevents the database crash.
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            // --- Existing Logic ---
            bool existsInWishlist = await _wishlistRepository.ExistsAsync(userId, productId);

            if (existsInWishlist)
            {
                await _wishlistRepository.RemoveAsync(userId, productId);
                return WishlistActionEnum.Removed;
            }
            else
            {
                await _wishlistRepository.AddAsync(userId, productId);
                return WishlistActionEnum.Added;
            }
        }

        public async Task<List<WishlistItemResponse>> GetWishlistAsync(int userId)
        {
            var products = await _wishlistRepository.GetWishlistByUserIdAsync(userId);

            // Map Entity -> DTO
            return products.Select(p => new WishlistItemResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category,
                Stock  = p.Stock,
                Images = p.Images?.Select(img => img.Url).ToList() ?? new List<string>()
                // Add other product fields you need
            }).ToList();
        }

        public async Task ClearWishlistAsync(int userId)
        {
            await _wishlistRepository.ClearAsync(userId);
        }

        public async Task RemoveFromWishlistAsync(int userId, int productId)
        {
            
            bool exists = await _wishlistRepository.ExistsAsync(userId, productId);
            if (exists)
            {
                await _wishlistRepository.RemoveAsync(userId, productId);
            }
        }
    }
    
}
