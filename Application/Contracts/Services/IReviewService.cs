using Application.Contracts.Repositories;
using Application.DTOs.Reviews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
   
        public interface IReviewService
        {
            Task<int> AddReviewAsync(int userId, CreateReviewRequest request);
            Task<List<ReviewResponse>> GetProductReviewsAsync(int productId);
            Task<bool> DeleteReviewAsync(int reviewId, int userId);
        }
    
}
