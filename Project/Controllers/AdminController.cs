using Application.Contracts.Services;
using Application.DTOs.Admin;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Project.WebAPI.Controllers
{
   
        [Authorize(Roles = "Admin")]
        [ApiController]
        [Route("api/admin")]
        public class AdminController : ControllerBase
        {
            // Inject the Service, not the Use Cases
            private readonly IUserService _userService;
            private readonly IOrderService _orderService;
             private readonly IProductService _productService;

            public AdminController(IUserService userService,IOrderService orderService,IProductService productService)
            {
                _userService = userService;
            _productService = productService;
            _orderService = orderService;
            }

            [HttpGet("users")]
            public async Task<IActionResult> GetUsers()
            {
                var response = await _userService.GetAllUsersAsync();
                return Ok(new ApiResponse<List<UserResponse>>(200, "Users fetched successfully", response));
            }

            [HttpGet("{id}")]
            public async Task<IActionResult> GetUserById(int id)
            {
                var response = await _userService.GetByIdAsync(id);

                if (response == null)
                {
                    return NotFound(new ApiResponse<string>(404, "User not found"));
                }

                return Ok(new ApiResponse<UserResponse>(200, "User fetched successfully", response));
            }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                return Ok(new ApiResponse<string>(200, "User Removed Successfully"));



            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ex.Message));
            }
        }

            [HttpPut("{id}/block-status")]
            public async Task<IActionResult> ToggleBlockStatus(int id)
            {
                var newStatus = await _userService.ToggleBlockStatusAsync(id);

                if (newStatus == null)
                {
                    return NotFound(new ApiResponse<string>(404, "User not found"));
                }

                string message = newStatus.Value ? "User has been blocked" : "User has been unblocked";
                return Ok(new ApiResponse<bool>(200, message, newStatus.Value));
            }


        [HttpGet("users/deleted")] // URL: api/admin/users/deleted
        public async Task<IActionResult> GetDeletedUsers()
        {
            var response = await _userService.GetDeletdUsersAsync();
            return Ok(new ApiResponse<List<UserResponse>>(200, "Deleted users fetched successfully", response));
        }

        [HttpGet("dashboard-stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // One single call to the service
            var stats = await _orderService.GetDashBoardAsync();
            return Ok(new ApiResponse<DashBoardResponse>(200, "Dashboard stats fetched", stats));
        }
    }
    }


