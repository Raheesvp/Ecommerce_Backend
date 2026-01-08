using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Admin;
using Application.DTOs.Auth;
using Application.DTOs.Profile;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly IFileService _fileService;

        private readonly IOrderRepository _orderRepository;



        public UserService(IUserRepository userRepository, IFileService fileService, IOrderRepository orderRepository)
        {
            _userRepository = userRepository;
            _fileService = fileService;
            _orderRepository = orderRepository;
        }

        // 1. Get All Users
        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select(u => new UserResponse
            {
                Id = u.Id,
                FullName = u.FullName, // Ensure your User entity has this or combine FirstName + LastName
                Email = u.Email,
                Role = u.Role.ToString(),
                IsBlocked = u.IsBlocked,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        // 2. Get User By ID
        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsBlocked = user.IsBlocked,
                CreatedAt = user.CreatedAt
            };
        }

        // 3. Delete User
        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            if (user.Role == Roles.Admin)
                throw new InvalidOperationException("Security Alert :You Cannot Block an Administrator");

            var userOrders = await _orderRepository.GetUserOrdersAsync(userId);

            if (userOrders.Any())
            {
                throw new InvalidOperationException("Cannot Delete User. This User has Purchase History");
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = "Admin";

            //await _userRepository.DeleteAsync(user);

            await _userRepository.UpdateAsync(user);
            return true;
        }

        // 4. Block/Unblock User
        public async Task<bool?> ToggleBlockStatusAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            if (!user.IsBlocked)
            {
                var orders = await _orderRepository.GetUserOrdersAsync(userId);

                bool hasCompleteOrders = orders.Any(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled);

                if (hasCompleteOrders)
                {
                    throw new InvalidOperationException("Action Denied: This user has active orders(Pending, Processing, or Shipped). " +
                "You must complete or cancel all orders before blocking the account.");
                }
            }

            if (user.Role == Roles.Admin)
                throw new InvalidOperationException("Security Alert :You Cannot Block an Administrator");

            user.IsBlocked = !user.IsBlocked; // Toggle status
            await _userRepository.UpdateAsync(user);

            return user.IsBlocked;

        }

        public async Task<List<UserResponse>> GetDeletdUsersAsync()
        {
            var users = await _userRepository.GetDeletedUsersAsync();

            return users.Select(u => new UserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsBlocked = u.IsBlocked,
                CreatedAt = u.CreatedAt,

                // You can also map "DeletedAt" here if you want to see when they were deleted
            }).ToList();
        }

        public async Task<UserProfile> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            return new UserProfile
            {
                Email = user.Email,
                FirstName = string.IsNullOrWhiteSpace(user.FirstName) ? null : user.FirstName,
                LastName = string.IsNullOrWhiteSpace(user.LastName) ? null : user.LastName,
                MobileNumber = string.IsNullOrWhiteSpace(user.MobileNumber) ? null : user.MobileNumber,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }



        public async Task UpdateUserProfileAsync(int userId, UpdateUserProfile request)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            if (request.FirstName != null)
                user.FirstName = request.FirstName.Trim();

            if (request.LastName != null)
                user.LastName = request.LastName.Trim();

            if (request.MobileNumber != null)
                user.MobileNumber = request.MobileNumber.Trim();

            if (request.ProfileImage != null)
            {
                var imageUrl = await _fileService.UploadAsync(request.ProfileImage);
                Console.WriteLine("UPLOAD URL => " + imageUrl);
                user.ProfileImageUrl = imageUrl;
            }

            await _userRepository.UpdateAsync(user);
        }


    }
}
