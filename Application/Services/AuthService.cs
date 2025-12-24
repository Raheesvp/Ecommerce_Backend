using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Auth;
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
        // RULE 2: Depend on Abstractions (Interfaces), not concrete classes
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService; // Ensure you have this interface too

        // Constructor Injection
        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            // Business Logic
            if (request.Email.Contains("example@gmail.com"))
                throw new UnauthorizedAccessException("Demo accounts cannot register.");

            if (request.Password != request.ConfirmPassword)
                throw new Exception("Passwords do not match.");

            if (await _userRepository.EmailExistsAsync(request.Email))
                throw new Exception("Email already registered.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

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
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid Credentials.");

            if (user.IsBlocked)
                throw new UnauthorizedAccessException("Account be Blocked ");

            bool isCorrectPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isCorrectPassword)
                throw new Exception("Invalid Credentials.");

            // Generate Tokens
            string accessToken = _jwtService.GenerateAccessToken(user);
            string refreshToken = _jwtService.GenerateRefreshToken();

            user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
            await _userRepository.UpdateAsync(user);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Role = user.Role.ToString()
            };
        }

        //service for the refresh token 

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // 1. Get the User ID from the EXPIRED Access Token
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
                throw new Exception("Invalid access token");

            // The "NameIdentifier" claim holds the ID we saved earlier
            var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                throw new Exception("Invalid user ID in token");

            // 2. Get the User from the Database
            var user = await _userRepository.GetByIdAsync(userId); // Assuming you have GetByIdAsync
            if (user == null)
                throw new Exception("User not found");

            // 3. SECURITY CHECK: Does the Refresh Token match the DB? Is it expired?
            if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            // 4. Generate NEW Tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // 5. Save the NEW Refresh Token (Revoke the old one)
            user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
            await _userRepository.UpdateAsync(user);

            // 6. Return the new keys
            return new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Role = user.Role.ToString()
            };
        }
    }
}
