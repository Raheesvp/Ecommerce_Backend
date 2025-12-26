using Domain.Exceptions; 
// You might need to import your custom exception namespace, e.g., using SmartServe.Domain.Exceptions;

namespace Project.WebAPI
{
    public class Middlewares
    {
        private readonly RequestDelegate _next;

        public Middlewares(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            // 1. Handle "Not Enough Stock" (Business Rule Violation)
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailureResponse(400, ex.Message)
                );
            }
            // 2. Handle "Product Not Found" (Data Missing)
            // Note: Ensure you have your NotFoundException class imported or defined
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailureResponse(404, ex.Message)
                );
            }
            // 3. Handle existing specific exceptions
            catch (UnauthorizedAccessException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailureResponse(401, ex.Message)
                );
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailureResponse(400, ex.Message)
                );
            }
            // 4. Fallback for actual server crashes
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                // Tip: In development, you might want to see 'ex.Message' here to debug unknown crashes
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailureResponse(500, "Internal server error")
                );
            }
        }
    }
}