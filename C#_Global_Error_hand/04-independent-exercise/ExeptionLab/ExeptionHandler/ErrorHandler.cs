using System.Net;
using Microsoft.AspNetCore.Diagnostics;
namespace TaskManeger.ErrorHandling;
public class GlobalExeptionHnadler : IExceptionHandler
{
    private readonly ILogger<GlobalExeptionHnadler> _logger;
    public GlobalExeptionHnadler(ILogger<GlobalExeptionHnadler> logger)
    {
        _logger = logger;
    }
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken token)
    {
        if(ex is InvalidOperationException invalid)
        {
            _logger.LogError(invalid, "Invalid operation: {Message}", invalid.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync
            (
                new
                {
                    error = "A data error occurred"
                }
            );
            return true;
        }
        _logger.LogError(ex, "An unexpected error occurred");
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsJsonAsync
            (
                new
                {
                    error = "A data error occurred"
                }
            );
            return true;
    }
}