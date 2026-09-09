using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Calendar;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/calendar")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IMediator _mediator;

    public CalendarController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<CalendarResultDto>> Get(
        [FromQuery] Guid organizationId, [FromQuery] Guid? projectId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _mediator.Send(new GetCalendarQuery(organizationId, projectId, from, to));
        return Ok(result);
    }

    [HttpGet("workload")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<WorkloadResultDto>> Workload(
        [FromQuery] Guid organizationId, [FromQuery] Guid? projectId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _mediator.Send(new GetWorkloadQuery(organizationId, projectId, from, to));
        return Ok(result);
    }
}
