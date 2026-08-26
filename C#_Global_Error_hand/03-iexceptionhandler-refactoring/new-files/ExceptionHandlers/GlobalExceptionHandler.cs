using System.Net;
using Microsoft.AspNetCore.Diagnostics;

namespace ExceptionHandlingLab.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Handle specific exception types if needed
        if (exception is InvalidOperationException invalidOp)
        {
            _logger.LogError(invalidOp, "Invalid operation: {Message}", invalidOp.Message);

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            
            await httpContext.Response.WriteAsJsonAsync(
                new { error = "A data error occurred. Please try again later." },
                cancellationToken);

            return true;
        }

        // Catch-all for unexpected exceptions
        _logger.LogError(exception, "An unexpected error occurred");

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        
        await httpContext.Response.WriteAsJsonAsync(
            new { error = "An unexpected error occurred. Please try again later." },
            cancellationToken);

        return true;
    }
}
