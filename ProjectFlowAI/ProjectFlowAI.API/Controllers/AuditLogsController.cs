using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.AuditLogs;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.AuditLogsView)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> List(
        [FromQuery] Guid? organizationId, [FromQuery] Guid? userId, [FromQuery] string? entityType,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "desc")
    {
        var result = await _mediator.Send(new ListAuditLogsQuery(organizationId, userId, entityType, from, to, page, pageSize, sortBy, sortDir));
        return Ok(result);
    }
}
