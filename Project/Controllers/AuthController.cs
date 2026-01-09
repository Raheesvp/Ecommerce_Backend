using Application.Contracts.Services;
using Application.DTOs.Auth;
using Application.DTOs.Password;
using Application.Services;
using Domain.Enums;
using Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Services;
using System.Security.Claims;

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


            if (result == null)
                return Unauthorized(new ApiResponse<string>(401, "Invalid Credentials"));

            //store refresh token in session (server-side)
            HttpContext.Session.SetString("RefreshToken", result.RefreshToken);

            //do not send refresh token to client so doesnot sent to the react frontend


            result.RefreshToken = null!;


            return Ok(new ApiResponse<LoginResponse>(200, "Login Successful", result));
        }
       
        [HttpPost("Refresh-Token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = HttpContext.Session.GetString("RefreshToken");
            Console.WriteLine($"RefreshToken From Session:{refreshToken}");

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(
                    new ApiResponse<string>(401, "Session expired. Please login again.")
                );
            }

            var expiredAccessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();

            var result = await _authService.RefreshTokenAsync(expiredAccessToken, refreshToken);

            if (result == null)
            {
                HttpContext.Session.Clear();
                return Unauthorized(
                    new ApiResponse<string>(401, "Invalid refresh token")
                );
            }

            // Rotate refresh token in session
            HttpContext.Session.SetString(
                "RefreshToken",
                result.RefreshToken
            );

            result.RefreshToken = null!;

            return Ok(new ApiResponse<LoginResponse>(
                200,
                "Token Refreshed Successfully",
                result
            ));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO reset)
        {
            // 1. Check if the DTO validation (Regex/Required) failed
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiResponse<IEnumerable<string>>(400, "Validation Failed", errors));
            }

            // 2. Business logic check for password matching
            if (reset.NewPassword != reset.ConfirmPassword)
            {
                return BadRequest(new ApiResponse<string>(400, "Passwords do not match"));
            }

            // 3. Call the service
            var isSuccess = await _authService.ResetPasswordAsync(reset);

            if (!isSuccess)
            {
                // This usually means the Token was wrong, Email didn't exist, or it Expired
                return BadRequest(new ApiResponse<string>(400, "Invalid or Expired Token"));
            }

            return Ok(new ApiResponse<string>(200, "Password Reset Successfully"));
        }


        [HttpPost("forgot-password")]

        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO request)
        {
            var token = await _authService.ForgotPasswordAsync(request.Email);

            if (token == null)
            {

                return BadRequest(new ApiResponse<string>(400, "Invalid Email"));
            }
            return Ok(new ApiResponse<string>(200, "Token Generated Successfully", token));
        }

        [Authorize(Roles = "User")]

        [HttpPost("Change-Password")]

        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {

                return Unauthorized(new ApiResponse<string>(401, "User not authenticated"));
            }

            int userId = int.Parse(userIdString);


            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);


            if (!result)
            {
                return BadRequest(new ApiResponse<string>(400, "Current Password is Incorrect"));
            }
            return Ok(new ApiResponse<string>(200, "Password Changed Successfully", null));
        }
        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
            {
                await _authService.RevokeRefreshTokenAsync(userId);
            }
            //remvoes the refresh tokne 

            HttpContext.Session.Clear();

            return Ok(new ApiResponse<string>(200, "Logged out successfully"));
        }



    }
}
