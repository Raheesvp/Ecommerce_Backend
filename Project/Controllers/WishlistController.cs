using Application.Contracts.Services;
using Application.DTOs.Wishlist; // Ensure you have this DTO
using Application.DTOs; // For ApiResponse
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Domain.Enums;

namespace Project.WebAPI.Controllers
{
    [Authorize(Roles = "User")] // Only logged-in users can use wishlist
    [ApiController]
    [Route("api/wishlist")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpPost("toggle/{productId:int}")]
        public async Task<IActionResult> Toggle(int productId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _wishlistService.ToggleWishlistAsync(userId, productId);

            // We return generic success because it could be "Added" or "Removed"

            var message = result == WishlistActionEnum.Added ? "Added To Wishlist Successfully" : "Removed From Wishlist Successfully";
            
            return Ok(new ApiResponse<string>(200,message));
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var wishlist = await _wishlistService.GetWishlistAsync(userId);

            return Ok(new ApiResponse<List<WishlistItemResponse>>(200, "Wishlist fetched successfully", wishlist));
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearWishlist()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _wishlistService.ClearWishlistAsync(userId);

            return Ok(new ApiResponse<string>(200, "Wishlist cleared successfully"));
        }
    }
}