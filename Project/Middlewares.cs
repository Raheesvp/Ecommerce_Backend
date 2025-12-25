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
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailureResponse(500, "Internal server error")
                );
            }
        }
    }
}
