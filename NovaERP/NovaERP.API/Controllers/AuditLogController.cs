using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/audit-logs")]
[Authorize(Policy = "audit-logs.view")]
public class AuditLogController : ApiControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogController(IAuditLogService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null) =>
        Ok(await _service.GetPagedAsync(OrgId, page, pageSize, search));
}
