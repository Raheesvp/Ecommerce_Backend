using Application.Contracts.Services;
using Application.DTOs.Cart;
using Application.DTOs; // For ApiResponse
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Project.WebAPI.Controllers
{
    [Authorize(Roles ="User")]
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _cartService.GetCartAsync(userId);
            return Ok(new ApiResponse<List<CartItemResponse>>(200, "Cart fetched successfully", result));
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _cartService.AddToCartAsync(userId, productId, quantity);
            return Ok(new ApiResponse<string>(200, "Item added to cart"));
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _cartService.UpdateQuantityAsync(userId, request);
            return Ok(new ApiResponse<string>(200, "Cart updated successfully"));
        }

        [HttpDelete("remove/{productId:int}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _cartService.RemoveFromCartAsync(userId, productId);
            return Ok(new ApiResponse<string>(200, "Item removed from cart"));
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _cartService.ClearCartAsync(userId);
            return Ok(new ApiResponse<string>(200, "Cart cleared successfully"));
        }
    }
}