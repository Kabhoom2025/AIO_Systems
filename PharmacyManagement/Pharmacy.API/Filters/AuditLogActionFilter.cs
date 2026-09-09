using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Filters;

public class AuditLogActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> SkippedControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "AuditLog"
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

        var orgIdClaim = user.FindFirst("organizationId")?.Value;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        if (!int.TryParse(orgIdClaim, out var orgId) || !int.TryParse(userIdClaim, out var userId))
            return;

        int? entityId = null;
        if (context.RouteData.Values.TryGetValue("id", out var idValue) && int.TryParse(idValue?.ToString(), out var parsedId))
            entityId = parsedId;

        await _writer.WriteAsync(orgId, userId, userName, method, action, controllerName, entityId);
    }

    private static string? MapAction(string httpMethod) => httpMethod.ToUpperInvariant() switch
    {
        "POST"   => "Create",
        "PUT"    => "Update",
        "PATCH"  => "Update",
        "DELETE" => "Delete",
        _        => null // GET and anything else isn't audited
    };
}
