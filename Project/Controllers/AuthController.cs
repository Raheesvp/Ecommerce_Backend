using Application.Contracts.Services;
using Application.DTOs.Auth;
using Application.Services;
using Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Project.WebAPI.Controllers
{
    [ApiController]
    [Route("api/Auth")]
    public class AuthController : ControllerBase
    {
        // 1. Define the Interface (Abstraction)
        private readonly IAuthService _authService;

        // 2. Inject the Interface in the Constructor
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest request)
        {
            // 3. Call the Service method
            await _authService.RegisterAsync(request);

            // Return success response
            return Ok(new ApiResponse<string>(200, "User registered successfully"));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 3. Call the Service method
            var result = await _authService.LoginAsync(request);

            // Return success response with data
            return Ok(new ApiResponse<LoginResponse>(200, "Login Successful", result));
        }
    }
}
