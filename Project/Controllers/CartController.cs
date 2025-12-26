using Application.Contracts.Services;
using Application.DTOs; 
using Application.DTOs.Cart;
using Domain.Exceptions;
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

          
            return Ok(ApiResponse<string>.SuccessResponse("Item added to cart", 200));
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _cartService.UpdateQuantityAsync(userId, request);

            return Ok(ApiResponse<string>.SuccessResponse("Cart updated successfully", 200));
        }

        [HttpDelete("remove/{productId:int}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _cartService.RemoveFromCartAsync(userId, productId);

            return Ok(ApiResponse<string>.SuccessResponse("Item removed from cart", 200));
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _cartService.ClearCartAsync(userId);

            return Ok(ApiResponse<string>.SuccessResponse("Cart cleared successfully", 200));
        }
    }
}