using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Reviews;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public ReviewService(
            IOrderRepository orderRepository,
            IReviewRepository reviewRepository,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment env)
        {
            _orderRepository = orderRepository;
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
            _env = env;
        }

        public async Task<int> AddReviewAsync(int userId, CreateReviewRequest request)
        {
            // 1. Verify Purchase
            // FIXED: AnyAsnc -> AnyAsync
            var canReview = await _orderRepository.AnyAsync(o =>
                o.UserId == userId &&
                o.Status == OrderStatus.Delivered &&
                o.OrderItems.Any(oi => oi.ProductId == request.ProductId));

            if (!canReview)
                throw new Exception("Tactical Error: You must receive the item before submitting intel (reviews).");

            var review = new Review
            {
                UserId = userId,
                ProductId = request.ProductId,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewImages = new List<ReviewImage>()
            };

            // 2. Process Image Uploads
            if (request.ReviewImages != null && request.ReviewImages.Count > 0)
            {
                string uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "reviews");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var file in request.ReviewImages)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    review.ReviewImages.Add(new ReviewImage { Url = "/uploads/reviews/" + fileName });
                }
            }

            await _reviewRepository.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();
            return review.Id;
        }

        // FIX FOR CS0535: Implementing GetProductReviewsAsync
        public async Task<List<ReviewResponse>> GetProductReviewsAsync(int productId)
        {
            var reviews = await _reviewRepository.GetByProductIdAsync(productId);

            return reviews.Select(r => new ReviewResponse
            {
                Id = r.Id,
                UserName = r.User?.FullName ?? "Unknown Operator", // Assuming User entity has FullName
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                ImageUrls = r.ReviewImages.Select(img => img.Url).ToList()
            }).ToList();
        }

        // FIX FOR CS0535: Implementing DeleteReviewAsync
        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            // Security Check: Only the owner can delete their review
            if (review == null || review.UserId != userId)
            {
                return false;
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}