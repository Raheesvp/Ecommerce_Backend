using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs; 
using Application.DTOs.Category;
using Application.DTOs.Product;
using Application.DTOs.Reviews;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Project.WebAPI.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        private readonly ICategoryRepository _categroyRepo;

        private readonly INotificationService _notificationService;

        private readonly IReviewService _reviewService;

        



        // CLEAN CONSTRUCTOR: Only one service needed now!
        public ProductController(IProductService productService, ICategoryRepository categoryRepository,INotificationService notificationService,IReviewService reviewService)
        {
            _productService = productService;
            _categroyRepo = categoryRepository;
            _notificationService = notificationService;
            _reviewService = reviewService;
            
           
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(new ApiResponse<List<ProductResponse>>(200, "Success", products));
        }

        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetById(int productId)
        {
            var product = await _productService.GetByIdAsync(productId);
            return Ok(new ApiResponse<ProductResponse>(200, "Success", product));
        }

        [HttpGet("category/{categoryId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(int  categoryId)
        {
            var category = await _categroyRepo.GetByIdAsync(categoryId);

            if (category == null) return NotFound();
            var products = await _productService.GetByCategoryAsync(category.Name);
            return Ok(new ApiResponse<List<ProductResponse>>(200, "Success", products));
        }

        [HttpGet("Paginated")]
        public async Task<IActionResult> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _productService.GetPaginatedAsync(pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<ProductResponse>>(200, "Success", result));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Create-Product-Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateProductRequest request)
        {
            await _productService.CreateAsync(request);
            return Ok(new ApiResponse<string>(201, "Product created successfully"));
        }

        [HttpPatch("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductRequest request)
        {
            await _productService.UpdateAsync(id, request);
            return Ok(new ApiResponse<string>(200, "Product updated successfully"));
        }

        [HttpDelete("delete-product/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok(new ApiResponse<string>(200, "Product archieved successfully"));
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _productService.SearchAsync(query);
            return Ok(new ApiResponse<List<ProductSearchResponse>>(200, "Success", result));
        }
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured()
        {
            var products = await _productService.GetFeaturedProductAsync();
            return Ok(new ApiResponse<List<ProductResponse>>(200, "Featured Products Fetched", products));
        }

        [HttpGet("Filtered-Products")]

        public async Task<IActionResult> GetProducts([FromQuery] ProductFilterRequest productFilter)
        {
            var result = await _productService.GetFilteredAsync(productFilter);
            return Ok(new ApiResponse<PagedResponse<ProductResponse>>(200, "Successfully Fetched Successfully",result));
        }

        [HttpGet("{productId:int}/related")]


        public async Task<IActionResult> GetRelatedProducts(int productId)
        {
            var related = await _productService.GetRelatedProductsAsync(productId);
            return Ok(new ApiResponse<List<ProductResponse>>(200, "Success", related));
        }

        [Authorize]
        [HttpPost("submit-review")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitReview([FromForm] CreateReviewRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var reviewId = await _reviewService.AddReviewAsync(userId, request);
            return Ok(new { Message = "Review Deployed", ReviewId = reviewId });
        }

        [HttpGet("{productId:int}/reviews")]
        [AllowAnonymous] // Everyone should see reviews
        public async Task<IActionResult> GetReviews(int productId)
        {
            
            var reviews = await _reviewService.GetProductReviewsAsync(productId);

            return Ok(new ApiResponse<List<ReviewResponse>>(200, "Review Fetched", reviews));
        }

        //get archieved products

        [HttpGet("archived-inventory")]
        [Authorize(Roles ="Admin")]

        public async Task<IActionResult>GetArchieved()
        {
            var archieved = await _productService.GetArchieveProductAsync();
            return Ok(new ApiResponse<List<ProductResponse>>(200, "Archieved Products Fetched",archieved));
        }


        //restore products

        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore (int id)
        {
            await _productService.RestoreAsync(id);

            return Ok(new ApiResponse<string>(200, "Product Restored to active inventory success"));
        }

        [HttpGet("debug/connection")]
        public IActionResult DebugConnection([FromServices] IConfiguration config)
        {
            var cs = config.GetConnectionString("DefaultConnection");
            return Ok(new { connectionString = cs });
        }







    }
}