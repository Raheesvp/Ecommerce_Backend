using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Auth;
using Application.DTOs.Password;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService; 

   
        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<User> RegisterAsync(RegisterRequest request)
        {

            if (request == null)
                throw new ArgumentException("Request body is required");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required");

            if (request.Password != request.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            if (await _userRepository.EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already registered");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User(
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email,
                passwordHash: passwordHash,
                role: Roles.User
            );


            await _userRepository.AddAsync(user);
            return user;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (request == null)
                throw new ArgumentException("Request body is required");

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and password are required");

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || user.IsBlocked)
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
            await _userRepository.UpdateAsync(user);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken, // TEMP, session will store it
                Role = user.Role.ToString()
            };
        }


        //service for the refresh token 




        public async Task<LoginResponse?> RefreshTokenAsync(
      string expiredAccessToken,
      string sessionRefreshToken)
        {
            if (string.IsNullOrWhiteSpace(expiredAccessToken))
                return null;

            if (string.IsNullOrWhiteSpace(sessionRefreshToken))
                return null;

            // 1. Extract principal from expired access token
            ClaimsPrincipal principal;
            try
            {
                principal = _jwtService.GetPrincipalFromExpiredToken(expiredAccessToken);
            }
            catch
            {
                return null;
            }

            // 2. Extract userId
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return null;

            // 3. Fetch user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            // 4. Validate refresh token
            if (user.RefreshToken != sessionRefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            // 5. Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // 6. Rotate refresh token
            user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
            await _userRepository.UpdateAsync(user);

            // 7. Return response
            return new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Role = user.Role.ToString()
            };
        }


        //reset password section ...

        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO reset)
        {
            var user = await _userRepository.GetByEmailAsync(reset.Email);

            if (user == null) return false;

            if(user.ResetPasswordToken!= reset.EmailToken || user.ResetPasswordExpiry < DateTime.Now)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(reset.NewPassword);

            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;

            await _userRepository.UpdateAsync(user);
            return true;
        }


        //forgot password section ...

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null) return null;

            var token = new Random().Next(100000, 999999).ToString();

            user.ResetPasswordToken = token;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);

            await _userRepository.UpdateAsync(user);

            return token;
        }


        //change password section..

        public async Task<bool> ChangePasswordAsync(int userId,string currentPassword,string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return false;
            }

           user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> RevokeRefreshTokenAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;

            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
