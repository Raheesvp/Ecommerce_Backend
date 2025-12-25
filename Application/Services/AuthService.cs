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

            var isCorrectPassword =
                BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isCorrectPassword)
                throw new UnauthorizedAccessException("Invalid credentials");

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

            if (request == null)
                throw new ArgumentException("Request body is required");

            if (string.IsNullOrWhiteSpace(request.AccessToken))
                throw new ArgumentException("Access token is required");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ArgumentException("Refresh token is required");

            ClaimsPrincipal principal;

            try
            {
                principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            }
            catch
            {
                throw new UnauthorizedAccessException("Invalid access token");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("Invalid token claims");

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            if (user.RefreshToken != request.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
            await _userRepository.UpdateAsync(user);

         
            return new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Role = user.Role.ToString()
            };
        }
    }
}
