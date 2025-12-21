using Application.Contracts.Repositories; // Ensure this matches your Interface namespace
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CartEntity>> GetCartByUserIdAsync(int userId)
        {
            return await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                    .ThenInclude(p => p.Images) // <--- CRITICAL FIX: Load images
                .ToListAsync();
        }

        // Renamed 'GetAsync' to 'GetCartItemAsync' for clarity
        public async Task<CartEntity?> GetCartItemAsync(int userId, int productId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

        public async Task AddItemAsync(CartEntity cart)
        {
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(CartEntity cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(CartEntity cart)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(int userId)
        {
            var items = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
            if (items.Any())
            {
                _context.Carts.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }
    }
}