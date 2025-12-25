using Application.Contracts.Services;
using Application.DTOs.Product;
using Application.DTOs; // For generic response types
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Project.WebAPI.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        // CLEAN CONSTRUCTOR: Only one service needed now!
        public ProductController(IProductService productService)
        {
            _productService = productService;
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

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var products = await _productService.GetByCategoryAsync(category);
            return Ok(new ApiResponse<List<ProductResponse>>(200, "Success", products));
        }

        [HttpGet("Paginated")]
        public async Task<IActionResult> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _productService.GetPaginatedAsync(pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<ProductResponse>>(200, "Success", result));
        }

        [HttpPost("Create-Product-Admin")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateProductRequest request)
        {
            await _productService.CreateAsync(request);
            return Ok(new ApiResponse<string>(201, "Product created successfully"));
        }

        [HttpPatch("id-Product-Update-Admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductRequest request)
        {
            await _productService.UpdateAsync(id, request);
            return Ok(new ApiResponse<string>(200, "Product updated successfully"));
        }

        [HttpDelete("id-Delete-Admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok(new ApiResponse<string>(200, "Product deleted successfully"));
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
    }
}