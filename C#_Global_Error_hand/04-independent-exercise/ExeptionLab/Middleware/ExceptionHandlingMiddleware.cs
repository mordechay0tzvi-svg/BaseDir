using System.Net;
namespace TaskManeger.Middleware;
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch(InvalidOperationException e)
        {
            _logger.LogError(e, "Invalid operarion {Message}", e.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync
            (
                new
                {
                    error = "A data error occurred."
                }
            );
        }
        catch(Exception e)
        {
            _logger.LogError(e, "An unexpected error occurred");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync
            (
                new
                {
                    error = "An unexpected error occurred."
                }
            );
        }
    }
}