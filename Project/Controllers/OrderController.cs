using Application.Contracts.Services;
using Application.DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Project.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Call the service
            int orderId = await _orderService.PlaceOrderAsync(userId, request);

            return Ok(new ApiResponse<int>(200, "Order placed successfully!", orderId));
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyOrders()
        {
            // 1. Get the logged-in user's ID from the Token
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 2. Call Service
            var orders = await _orderService.GetUserOrdersAsync(userId);

            // 3. Return Result
            return Ok(new ApiResponse<List<OrderResponse>>(200, "Orders fetched successfully", orders));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                // 1. Get User ID from Token
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // 2. Call Service
                var order = await _orderService.GetOrderByIdAsync(userId, id);

                // 3. Return Result
                return Ok(new ApiResponse<OrderResponse>(200, "Order details fetched successfully", order));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Order not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { message = "You are not allowed to view this order." });
            }
        }
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(new ApiResponse<List<OrderResponse>>(200, "All orders retrieved", orders));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ex.Message));
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusDTO request)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, request.Status);

            if (!result)
            {
                return NotFound("Order not found");
            }

            return Ok(new ApiResponse<OrderResponse>(200, "Order status updated successfully!"));
        }

        [HttpPost("cancel/{orderId}")]
        [Authorize(Roles = "User")]

        [Authorize]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _orderService.CancelOrderAsync(orderId, userId);
            return Ok(new ApiResponse<string>(200, "Order Cancelled Successfully"));
        }
    }
}

