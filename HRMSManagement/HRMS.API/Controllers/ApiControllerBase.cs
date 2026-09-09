using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Organization id from the JWT; 0 when unauthenticated.</summary>
    protected int OrgId =>
        int.TryParse(User.FindFirst("organizationId")?.Value, out var v) ? v : 0;

    protected int UserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var v) ? v : 0;

    /// <summary>Employee profile linked to the logged-in user, if any.</summary>
    protected int? EmployeeId =>
        int.TryParse(User.FindFirst("employeeId")?.Value, out var v) ? v : null;

    protected string UserName => User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
}
