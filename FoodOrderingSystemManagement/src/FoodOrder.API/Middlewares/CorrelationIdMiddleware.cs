namespace FoodOrder.API.Middlewares;

/// <summary>
/// Assigns a unique correlation ID to every request.
/// If the client sends X-Correlation-Id, that value is used (useful for end-to-end tracing).
/// If not, a new GUID is generated.
/// The ID is echoed back in the response header and stored in HttpContext.Items
/// so all downstream middleware and services can reference it.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await _next(context);
    }
}
