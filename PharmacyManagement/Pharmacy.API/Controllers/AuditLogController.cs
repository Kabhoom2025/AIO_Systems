using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogController(IAuditLogService service) => _service = service;

    [Authorize(Policy = "audit-logs.view")]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetRecent(int orgId, [FromQuery] int take = 200) =>
        Ok(await _service.GetRecentAsync(orgId, take));
}
