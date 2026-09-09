using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AIO_Systems.Controllers;

/// <summary>
/// Shared claim-reading helpers for controllers, mirroring FoodOrder.API's BaseApiController
/// so the same claim names ("organizationId", ClaimTypes.*) are read consistently across services.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    protected int? GetCurrentOrganizationId()
    {
        var value = User.FindFirstValue("organizationId");
        return int.TryParse(value, out var id) ? id : null;
    }

    protected string? GetCurrentUserRole() => User.FindFirstValue(ClaimTypes.Role);

    protected string? GetCurrentUserName() => User.FindFirstValue(ClaimTypes.Name);
}
