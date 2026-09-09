using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Filters;

/// <summary>Writes an audit entry for every successful authenticated mutation (POST/PUT/PATCH/DELETE).</summary>
public class AuditLogActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> SkippedControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "AuditLog", "Auth"
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

        var sw = Stopwatch.StartNew();
        var executed = await next();
        sw.Stop();

        if (executed.Exception != null || executed.Result is not IStatusCodeActionResult statusResult)
            return;

        var statusCode = statusResult.StatusCode ?? 200;
        if (statusCode < 200 || statusCode >= 300) return;

        var orgId = int.TryParse(user.FindFirst("organizationId")?.Value, out var o) ? o : (int?)null;
        var userId = int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var u) ? u : (int?)null;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value;
        var path = context.HttpContext.Request.Path.ToString();
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        await _writer.WriteAsync(orgId, userId, userName, method, path,
            $"{action} {controllerName}", statusCode, ip, sw.ElapsedMilliseconds);
    }

    private static string? MapAction(string httpMethod) => httpMethod.ToUpperInvariant() switch
    {
        "POST"   => "Create",
        "PUT"    => "Update",
        "PATCH"  => "Update",
        "DELETE" => "Delete",
        _        => null // GET is not audited
    };
}
