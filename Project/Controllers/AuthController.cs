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
  
        private readonly IAuthService _authService;

       
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest request)
        {
           
            await _authService.RegisterAsync(request);
            return StatusCode(
                 StatusCodes.Status201Created,
                  new ApiResponse<string>(201, "User registered successfully"));

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
           
            var result = await _authService.LoginAsync(request);
            return Ok(new ApiResponse<LoginResponse>(200, "Login Successful", result));
        }

        [HttpPost("Refresh-Token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(new ApiResponse<LoginResponse>(200, "Token Refreshed Successfully", result));
        }
    }
}
