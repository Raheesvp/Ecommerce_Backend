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
     
        Task<User> RegisterAsync(RegisterRequest request);

 
        Task<LoginResponse> LoginAsync(LoginRequest request);
    
        Task<LoginResponse> RefreshTokenAsync(string  expiredAccessToken,string sessionRefreshToken);

        Task<bool> ResetPasswordAsync(ResetPasswordDTO resetDto);

        Task<string> ForgotPasswordAsync(string email);

        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        Task<bool> RevokeRefreshTokenAsync(int userId);
    }
}
