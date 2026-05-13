using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "AppException caught: {Message}", ex.Message);
                context.Response.StatusCode = ex.StatusCode;
                await context.Response.WriteAsJsonAsync(ApiResponse<object>.FailureResult(ex.Message, ex.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(ApiResponse<object>.FailureResult("An internal server error occurred.", new List<string> { ex.Message }));
            }
        }
    }
}