using Application.Contracts.Services;
using Application.DTOs.Order;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
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

        private readonly INotificationService _notificationService;

        private readonly IPdfService _pdfService;

     

        public OrderController(IOrderService orderService,INotificationService notificationService,IPdfService pdfService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
            _pdfService = pdfService;
         
            
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

          
            int orderId = await _orderService.PlaceOrderAsync(userId, request);

            await _notificationService.OrderPlacedAsync($"New Order #{orderId} placed!", orderId);  

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
               
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message = ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        message = ex.InnerException.InnerException.Message;
                    }
                }

            
                Console.WriteLine("CRITICAL SQL ERROR: " + message);
                return BadRequest(new { error = message });
            }
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyOrders()
        {
        
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

         
            var orders = await _orderService.GetUserOrdersAsync(userId);

          
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
           
            var order = await _orderService.GetOrderByIdAsync(request.OrderId);
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {request.OrderId} was not found." });
            }

           
            if (!string.Equals(request.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Payment was not successful. Status received: " + request.Status });
            }

        
            var success = await _orderService.VerifyOrderPaymentAsync(request);

            if (success)
            {
                return Ok(new { message = "Payment Successful and Order Updated" });
            }

           
            return StatusCode(500, new { message = "Database error: Could not update the order record." });
        }

        [HttpGet("invoice/{orderId}")] 
        public async Task<IActionResult> DownloadInvoice(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null) return NotFound("Order not found");

                var pdfBytes = await _pdfService.GenerateInvoicePdf(order);

                if (pdfBytes == null || pdfBytes.Length == 0)
                    return StatusCode(500, "Invoice generation failed (empty bytes)");

                return File(pdfBytes, "application/pdf", $"WolfAthletix_Invoice_{orderId}.pdf");
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"PDF GENERATION ERROR: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        
        [HttpPost("{id}/return")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ReturnOrder(int id, [FromBody] CreateReturnRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Call the service
            var returnId = await _orderService.CreateReturnRequestAsync(userId, id, request);

            return Ok(new ApiResponse<int>(200, "Return request submitted!", returnId));
        }

        [HttpGet("returns/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReturnRequests()
        {
            try
            {
                var returns = await _orderService.GetAllReturnRequestsAsync();
                return Ok(new ApiResponse<List<ReturnResponse>>(200, "All return requests retrieved", returns));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ex.Message));
            }
        }
    }
}



