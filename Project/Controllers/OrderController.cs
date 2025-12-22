using Application.Contracts.Services;
using Application.DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Project.WebAPI.Controllers
{
        [Authorize(Roles ="User")]
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
            public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderRequest request)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // Call the service
                int orderId = await _orderService.PlaceOrderAsync(userId, request);

                return Ok(new ApiResponse<int>(200, "Order placed successfully!", orderId));
            }

        [HttpGet("my-orders")]
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
    }
    }

