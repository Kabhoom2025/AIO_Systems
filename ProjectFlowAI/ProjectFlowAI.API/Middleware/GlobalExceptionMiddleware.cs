using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Middleware;

/// <summary>Maps every known exception type to an RFC 7807 ProblemDetails response and hides
/// internals from clients for anything unexpected (mirrors NovaERP's GlobalExceptionMiddleware,
/// adapted to ProblemDetails output and this app's own exception hierarchy).</summary>
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
            var (statusCode, title, detail) = ex switch
            {
                ValidationException ve => (StatusCodes.Status400BadRequest, "Validation failed",
                    string.Join(" ", ve.Errors.Select(e => e.ErrorMessage))),
                AiNotConfiguredException => (StatusCodes.Status503ServiceUnavailable, "AI not configured", ex.Message),
                NotFoundException => (StatusCodes.Status404NotFound, "Not found", ex.Message),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict", ex.Message),
                UnauthorizedDomainException => (StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message),
                OAuthVerificationException => (StatusCodes.Status401Unauthorized, "OAuth verification failed", ex.Message),
                DomainException => (StatusCodes.Status400BadRequest, "Bad request", ex.Message),
                // Restrict-delete FKs (e.g. deleting a Department that still has Teams pointing at
                // it) surface as a raw Postgres error inside DbUpdateException. Without this case
                // every such delete 500s with no useful message instead of a clean, expected conflict.
                DbUpdateException { InnerException: PostgresException { SqlState: "23503" } fkEx } =>
                    (StatusCodes.Status409Conflict, "Conflict",
                        $"This can't be deleted because other {(fkEx.TableName ?? "records")} still reference it. Remove or reassign those first."),
                DbUpdateException { InnerException: PostgresException { SqlState: "23505" } } =>
                    (StatusCodes.Status409Conflict, "Conflict", "A record with the same unique value already exists."),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "Please try again later.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted) throw;

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
