using Application.DTOs.Auth;
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
    }
}
