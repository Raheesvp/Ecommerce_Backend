using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repositories
{
    public interface IReviewRepository
    {
        Task<Review> GetByIdAsync(int id);
        Task<List<Review>> GetByProductIdAsync(int productId);
        Task<Review> GetAsync(Expression<Func<Review, bool>> predicate);
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
    }
}
