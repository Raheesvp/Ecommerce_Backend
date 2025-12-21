using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Admin;
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

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            if (user.Role == Roles.Admin)
                throw new InvalidOperationException("Security Alert :You Cannot Block an Administrator");


            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = "Admin";

            //await _userRepository.DeleteAsync(user);

            await _userRepository.UpdateAsync(user);
            return true;
        }

        // 4. Block/Unblock User
        public async Task<bool?> ToggleBlockStatusAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

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
    }
    }
