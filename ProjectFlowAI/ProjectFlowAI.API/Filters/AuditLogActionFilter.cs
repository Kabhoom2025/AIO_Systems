using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Filters;

/// <summary>Writes an audit entry for every successful authenticated mutation (POST/PUT/PATCH/DELETE),
/// mirroring NovaERP.API.Filters.AuditLogActionFilter.</summary>
public class AuditLogActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> SkippedControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "AuditLogs", "Auth"
    };

    private readonly IAuditLogWriter _writer;

    public AuditLogActionFilter(IAuditLogWriter writer) => _writer = writer;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        var action = MapAction(method);
        var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
        var user = context.HttpContext.User;

        if (action == null || SkippedControllers.Contains(controllerName) || user.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        var executed = await next();

        if (executed.Exception != null || executed.Result is not IStatusCodeActionResult statusResult)
            return;

        var statusCode = statusResult.StatusCode ?? 200;
        if (statusCode < 200 || statusCode >= 300) return;

        var orgId = Guid.TryParse(user.FindFirst("organizationId")?.Value, out var o) ? o : (Guid?)null;
        var userId = Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var u) ? u : (Guid?)null;
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        await _writer.WriteAsync(orgId, userId, $"{action} {controllerName}", controllerName, null, null, ip);
    }

    private static string? MapAction(string httpMethod) => httpMethod.ToUpperInvariant() switch
    {
        "POST" => "Create",
        "PUT" => "Update",
        "PATCH" => "Update",
        "DELETE" => "Delete",
        _ => null
    };
}
