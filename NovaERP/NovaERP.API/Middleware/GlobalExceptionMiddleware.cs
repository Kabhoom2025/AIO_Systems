using System.Text.Json;
using FluentValidation;

namespace NovaERP.API.Middleware;

/// <summary>Maps well-known service exceptions to HTTP status codes and hides internals from clients.</summary>
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
            var (statusCode, message) = ex switch
            {
                ValidationException          => (StatusCodes.Status400BadRequest, string.Join(" ", ((ValidationException)ex).Errors.Select(e => e.ErrorMessage))),
                KeyNotFoundException          => (StatusCodes.Status404NotFound, ex.Message),
                InvalidOperationException     => (StatusCodes.Status400BadRequest, ex.Message),
                ArgumentException             => (StatusCodes.Status400BadRequest, ex.Message),
                UnauthorizedAccessException   => (StatusCodes.Status403Forbidden, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted) throw;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
        }
    }
}
