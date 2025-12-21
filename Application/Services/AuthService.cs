using Application.Contracts.Services;
using Application.Contracts.Repositories;
using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public  class AuthService :IAuthService
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

            if (user == null )
                throw new UnauthorizedAccessException("Invalid Credentials.");

            if (user.IsBlocked)
                throw new  UnauthorizedAccessException("Account be Blocked ");

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
    }
}
