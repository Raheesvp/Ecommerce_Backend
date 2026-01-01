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

   

        public UserService(IUserRepository userRepository,IFileService fileService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
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

        public async Task<UserProfile> GetUserProfileAsync(int userId)
        {
            // 1. Fetch User
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // 2. Map to DTO
            return new UserProfile
            {
                //Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MobileNumber = user.MobileNumber,
                ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageUrl)
            ? "https://your-default-image-url.com/avatar.png"
            : user.ProfileImageUrl

                // Combine names if your entity has them, or just map what you have
                // FullName = $"{user.FirstName} {user.LastName}" 
            };
        }

        public async Task UpdateUserProfileImageAsync(int userId, string imageUrl)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            // Save the URL to the User entity
            user.ProfileImageUrl = imageUrl;

            await _userRepository.UpdateAsync(user);
        }

        public async Task UpdateUserProfileAsync(int userId, UpdateUserProfile request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            // 1. Update Basic Info
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.MobileNumber = request.MobileNumber;

            // 2. Handle Image (If provided)
            if (request.ProfileImage != null)
            {
                string imageUrl = await _fileService.UploadAsync(request.ProfileImage);
                user.ProfileImageUrl = imageUrl;
            }

            // 3. Handle Password (Only if user typed something in "New Password")
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                // Security Check: Did they provide the OLD password?
                if (string.IsNullOrEmpty(request.CurrentPassword))
                {
                    throw new Exception("You must provide your current password to set a new one.");
                }

                // Verify Old Password
                bool isCorrect = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
                if (!isCorrect)
                {
                    throw new Exception("Current password is incorrect.");
                }

                // Hash and Save New Password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            // 4. Save Everything
            await _userRepository.UpdateAsync(user);
        }
        //change the password

        //public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request)
        //{
        //    var user = await _userRepository.GetByIdAsync(userId);
        //    if (user == null) throw new KeyNotFoundException("User not found");


        //    bool isCorrect = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);




        //    if (!isCorrect)
        //    {
        //        throw new Exception("Current password is incorrect.");
        //    }


        //    string newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        //    user.PasswordHash = newHash;

        //    await _userRepository.UpdateAsync(user);
        //    return true;
        //}
    }
  }
