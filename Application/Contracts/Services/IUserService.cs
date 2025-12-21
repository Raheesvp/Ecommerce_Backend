using Application.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public interface IUserService
    {
        Task<List<UserResponse>> GetAllUsersAsync();
        Task<UserResponse?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool?> ToggleBlockStatusAsync(int id);

        Task<List<UserResponse>> GetDeletdUsersAsync();

    }
}
