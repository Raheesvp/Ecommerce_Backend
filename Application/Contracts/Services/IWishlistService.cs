using Application.DTOs.Wishlist;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public  interface IWishlistService
    {
        Task<WishlistActionEnum> ToggleWishlistAsync(int userId, int productId);
        Task ClearWishlistAsync(int userId);
        Task<List<WishlistItemResponse>> GetWishlistAsync(int userId);
    }
}
