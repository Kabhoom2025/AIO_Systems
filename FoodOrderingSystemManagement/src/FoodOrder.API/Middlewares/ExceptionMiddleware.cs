using FoodOrder.Shared.Exceptions;
using FoodOrder.Shared.Responses;

namespace FoodOrder.API.Middlewares;

/// <summary>
/// Global exception handler — must be the first middleware in the pipeline.
///
/// Catch hierarchy:
///   AppException (and subclasses)  → structured JSON at the declared status code, Warning log.
///   OperationCanceledException      → 499 Client Closed Request, no error log (client disconnected).
///   Any other Exception             → HTTP 500, Error log. Stack trace is never exposed to the client.
///
/// Every response includes the X-Correlation-Id header (set by CorrelationIdMiddleware)
/// so support staff can trace a request across logs.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (AppException ex)
        {
            var correlationId = GetCorrelationId(context);

            _logger.LogWarning(
                "AppException [{CorrelationId}] | {StatusCode} | {Path} | {Message}",
                correlationId, ex.StatusCode, context.Request.Path, ex.Message);

            await WriteJsonAsync(context, ex.StatusCode,
                ApiResponse<object>.FailureResult(ex.Message));
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — not an error, do not log as error.
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            var correlationId = GetCorrelationId(context);

            _logger.LogError(ex,
                "Unhandled exception [{CorrelationId}] | {Path}",
                correlationId, context.Request.Path);

            await WriteJsonAsync(context, StatusCodes.Status500InternalServerError,
                ApiResponse<object>.FailureResult(
                    $"An unexpected error occurred. Reference ID: {correlationId}"));
        }
    }

    private static string GetCorrelationId(HttpContext context) =>
        context.Items[CorrelationIdMiddleware.HeaderName]?.ToString()
        ?? context.Response.Headers[CorrelationIdMiddleware.HeaderName].FirstOrDefault()
        ?? Guid.NewGuid().ToString();

    private static async Task WriteJsonAsync<T>(HttpContext context, int statusCode, T response)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
