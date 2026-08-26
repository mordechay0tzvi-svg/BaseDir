using System.Net;

namespace ExceptionHandlingLab.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            // Example: Repository/data layer threw an unexpected exception
            _logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "A data error occurred. Please try again later." 
            });
        }
        catch (Exception ex)
        {
            // Catch-all for any other unexpected exceptions
            _logger.LogError(ex, "An unexpected error occurred");
            
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "An unexpected error occurred. Please try again later." 
            });
        }
    }
}
