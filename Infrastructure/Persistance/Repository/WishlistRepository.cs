
using Application.Contracts.Repositories;
using Domain.Entities;
using Infrastructure.Persistance;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repository
{
    public  class WishlistRepository: IWishlistRepository
    {
        private readonly AppDbContext _context;

        public WishlistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int userId,int productId)
        {
            return await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task AddAsync(int userId,int productId)
        {
            var wishlist = new WishlistEntity(userId, productId);
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();
        }

        //remove from wishlist if it is already in the table 

        public async Task RemoveAsync(int userId,int productId)
        {
            var entity = await _context.Wishlists
          .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (entity == null)
                return; 

            _context.Wishlists.Remove(entity);
            await _context.SaveChangesAsync();
        }

        //get the added wishlist items 

        public async Task<List<Product>> GetWishlistByUserIdAsync(int userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                .ThenInclude(w=>w.Images)
                .Select(w => w.Product)
                .ToListAsync();
        }

        //clear entire wishlist items 

        public async Task ClearAsync(int userId)
        {
            var items = await _context.Wishlists.Where(i => i.UserId == userId).ToListAsync();

            if (!items.Any())
            {
                return;
            }
            _context.Wishlists.RemoveRange(items);
            await _context.SaveChangesAsync();
        }


    }
}
