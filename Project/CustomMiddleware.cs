using Application.Contracts.Repositories;
using System.Security.Claims;

public class UserBlockMiddleware
{
    private readonly RequestDelegate _next;

    public UserBlockMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        
        if (context.User.Identity?.IsAuthenticated == true)
        {
           
            var userRepository = context.RequestServices.GetRequiredService<IUserRepository>();

            var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await userRepository.GetByIdAsync(userId);

            
                if (user == null || user.IsBlocked)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "User Blocked"
                    });
                    return;
                }
            }
        }

      
        await _next(context);
    }
}