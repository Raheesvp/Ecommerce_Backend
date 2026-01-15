using Application.Contracts.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Review> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.ReviewImages)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Review>> GetByProductIdAsync(int productId)
        {
            return await _context.Reviews
                .Include(r => r.ReviewImages)
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review> GetAsync(Expression<Func<Review, bool>> predicate)
        {
            return await _context.Reviews.FirstOrDefaultAsync(predicate);
        }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null) _context.Reviews.Remove(review);
        }
    }
}

