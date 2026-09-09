using System.Net;
using System.Text.Json;

namespace FlowSphere.API.Middleware;

/// <summary>Catches truly unexpected exceptions only - Result<T> handles expected failure paths
/// (validation/not-found/conflict) without throwing, so anything reaching here is a bug.</summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Path}", context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var payload = JsonSerializer.Serialize(new
            {
                error = new { code = "Unexpected", message = "An unexpected error occurred." }
            });

            await context.Response.WriteAsync(payload);
        }
    }
}
