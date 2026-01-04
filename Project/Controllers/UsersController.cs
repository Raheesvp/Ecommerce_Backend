using Application.Contracts.Services;
using Application.DTOs.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim.Value);
            var userProfile = await _userService.GetUserProfileAsync(userId);

            return Ok(userProfile);
        }

       
        [HttpPut("profile-update")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfile request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                int userId = int.Parse(userIdClaim.Value);

                await _userService.UpdateUserProfileAsync(userId, request);

                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }
    }
}