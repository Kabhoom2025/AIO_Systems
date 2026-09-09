using System.Net;
using System.Text.Json;
using AIO_Systems.Shared;

namespace AIO_Systems.Middlewares;

/// <summary>
/// Central exception handler: catches AppException (and subclasses like ForbiddenException)
/// and writes them as ApiResponse&lt;object&gt; with the exception's own StatusCode; anything
/// else is treated as an unhandled 500.
/// </summary>
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            logger.LogWarning(ex, "Handled application exception: {Message}", ex.Message);
            await WriteResponseAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteResponseAsync(context, (int)HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteResponseAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        var payload = ApiResponse<object>.FailureResult(message);
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
