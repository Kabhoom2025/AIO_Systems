using System.Diagnostics;
using System.Security.Claims;

namespace FoodOrder.API.Middlewares;

/// <summary>
/// Logs every HTTP request with method, path, status code, response time, and user identity.
/// Sits after ExceptionMiddleware so the final status code (including error responses) is captured.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var correlationId = context.Items[CorrelationIdMiddleware.HeaderName]?.ToString() ?? "-";

        _logger.LogInformation(
            "HTTP {Method} {Path} | {StatusCode} | {ElapsedMs}ms | User: {UserId} | CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            userId,
            correlationId);
    }
}
