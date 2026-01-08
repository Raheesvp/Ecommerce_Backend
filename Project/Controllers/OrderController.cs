using Application.Contracts.Services;
using Application.DTOs.Order;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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



        [HttpPost("direct-buy")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DirectBuy([FromBody] DirectBuyRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                
                var orderId = await _orderService.ProcessDirectBuyAsync(userId, request);

                return Ok(new ApiResponse<OrderResponse>(200, "Direct order created", orderId));
            }
            catch (Exception ex)
            {
                // EF Core wraps the real error deep inside. This code finds it.
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message = ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        message = ex.InnerException.InnerException.Message;
                    }
                }

                // This will print the EXACT SQL error like "Column 'City' does not exist"
                Console.WriteLine("CRITICAL SQL ERROR: " + message);
                return BadRequest(new { error = message });
            }
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
               
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

         
                var order = await _orderService.GetOrderByIdAsync(id);

              
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
            try
            {

            var result = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                return Ok(new ApiResponse<string>(200, "Order Status Updated Successfully"));
            }catch(InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ex.Message)); 

            }

          

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

        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationRequest request)
        {
            // 1. First, check if the order exists at all (helps with debugging)
            var order = await _orderService.GetOrderByIdAsync(request.OrderId);
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {request.OrderId} was not found." });
            }

            // 2. Check if the status sent is actually 'Success' (ignoring case)
            if (!string.Equals(request.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Payment was not successful. Status received: " + request.Status });
            }

            // 3. Call the service to update database (Status, Reference, PaidOn)
            var success = await _orderService.VerifyOrderPaymentAsync(request);

            if (success)
            {
                return Ok(new { message = "Payment Successful and Order Updated" });
            }

            // 4. If it reaches here, it's likely a database column issue (like the 'PaidOn' error)
            return StatusCode(500, new { message = "Database error: Could not update the order record." });
        }
    }
}

