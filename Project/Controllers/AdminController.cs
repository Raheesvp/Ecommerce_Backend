using Application.Contracts.Services;
using Application.DTOs.Admin;
using Application.DTOs.Category;
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
            // Inject the Service
            private readonly IUserService _userService;
            private readonly IOrderService _orderService;
             private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

            public AdminController(IUserService userService,IOrderService orderService,IProductService productService,ICategoryService categoryService)
            {
            _userService = userService;
            _productService = productService;
            _orderService = orderService;
            _categoryService = categoryService;
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

            [HttpPut("block-status/{id}")]
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


        [HttpGet("users/deleted")] 
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

        //category view     

       
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            // Must pass 'result' as the third parameter
            return Ok(new ApiResponse<List<CategoryResponse>>(200, "Categories Fetched", result));
        }

       
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
                return BadRequest(new ApiResponse<string>(400, "Category name is required"));

            var category = await _categoryService.CreateCategoryAsync(request.Name);
            return Ok(new ApiResponse<CategoryResponse>(201, "Category Created Successfully", category));
        }

      
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryRequest request)
        {
            var updated = await _categoryService.UpdateCategoryAsync(id, request.Name);
            return Ok(new ApiResponse<CategoryResponse>(200, "Category Updated", updated));
        }

       
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return Ok(new ApiResponse<string>(200, "Category and all its products removed"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ex.Message));
            }
        }
    }
    }


