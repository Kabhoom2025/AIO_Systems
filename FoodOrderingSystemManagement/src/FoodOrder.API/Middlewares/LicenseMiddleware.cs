using System.Security.Claims;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.API.Middlewares;

/// <summary>
/// Validates that the authenticated organization user's license is active on every request.
/// SuperAdmin users (no organizationId claim) are always allowed through.
/// Auth and health endpoints are skipped so the login flow itself isn't blocked.
/// </summary>
public class LicenseMiddleware(RequestDelegate next)
{
    private static readonly string[] SkippedPrefixes =
        ["/api/auth", "/health", "/swagger", "/hubs"];

    public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Always allow public / infrastructure endpoints
        if (SkippedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        // Skip unauthenticated requests — auth middleware handles those
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        // SuperAdmin has no organizationId claim — skip license check
        var orgClaim = context.User.FindFirst("organizationId");
        if (orgClaim == null || !int.TryParse(orgClaim.Value, out var orgId))
        {
            await next(context);
            return;
        }

        var license = await licenseService.GetByOrgAsync(orgId);

        if (license == null)
            throw new ForbiddenException(
                "No license has been assigned to your organization. Please contact your system administrator.");

        if (license.Status == "Expired" || license.ExpiryDate < DateTime.UtcNow)
            throw new ForbiddenException(
                "Your organization's license has expired. Please contact your system administrator to renew.");

        await next(context);
    }
}
