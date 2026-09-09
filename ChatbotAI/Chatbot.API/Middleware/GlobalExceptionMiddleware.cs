using System.Net;
using System.Text.Json;
using Chatbot.Application.Common.Exceptions;

namespace Chatbot.API.Middleware;

public record ApiErrorResponse(bool Success, string Message, string ErrorCode, IDictionary<string, string[]>? Errors = null);

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationAppException validation => (HttpStatusCode.BadRequest,
                new ApiErrorResponse(false, exception.Message, validation.ErrorCode, validation.Errors)),
            NotFoundException notFound => (HttpStatusCode.NotFound,
                new ApiErrorResponse(false, exception.Message, notFound.ErrorCode)),
            UnauthorizedAppException unauthorized => (HttpStatusCode.Unauthorized,
                new ApiErrorResponse(false, exception.Message, unauthorized.ErrorCode)),
            ConflictAppException conflict => (HttpStatusCode.Conflict,
                new ApiErrorResponse(false, exception.Message, conflict.ErrorCode)),
            AiServiceException aiService => (HttpStatusCode.BadGateway,
                new ApiErrorResponse(false, "Unable to connect to the AI service. Please try again.", aiService.ErrorCode)),
            _ => (HttpStatusCode.InternalServerError,
                new ApiErrorResponse(false, "Unable to process your request.", "INTERNAL_ERROR"))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            logger.LogWarning("{ErrorCode}: {Message}", response.ErrorCode, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
