using Domain.Exceptions;
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
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailureResponse(400, ex.Message)
                );
            }
           

            catch (KeyNotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailureResponse(404, ex.Message)
                );
            }
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
           
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailureResponse(500, ex.Message)
                );
            }
             
        }
    }
}