using Application.DTOs.Auth;
using Application.DTOs.Password;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public  interface IAuthService
    {
        // 1. Contract for Registration
        Task<User> RegisterAsync(RegisterRequest request);

        // 2. Contract for Login
        Task<LoginResponse> LoginAsync(LoginRequest request);
        // service interface for the refresh token 
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);

        Task<bool> ResetPasswordAsync(ResetPasswordDTO resetDto);

        Task<string> ForgotPasswordAsync(string email);

        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        Task<bool> RevokeRefreshTokenAsync(int userId);
    }
}
