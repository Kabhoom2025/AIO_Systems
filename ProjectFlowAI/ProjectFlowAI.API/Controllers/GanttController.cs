using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Gantt;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/gantt")]
[Authorize]
public class GanttController : ControllerBase
{
    private readonly IMediator _mediator;

    public GanttController(IMediator mediator) => _mediator = mediator;

    public record CreateBaselineRequest(string Name);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<GanttChartDto>> GetChart(Guid projectId)
    {
        var result = await _mediator.Send(new GetGanttChartQuery(projectId));
        return Ok(result);
    }

    [HttpGet("critical-path")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<CriticalPathDto>> CriticalPath(Guid projectId)
    {
        var result = await _mediator.Send(new GetCriticalPathQuery(projectId));
        return Ok(result);
    }

    [HttpPost("baselines")]
    [Authorize(Policy = PermissionCatalog.SprintsManage)]
    public async Task<ActionResult<GanttBaselineDto>> CreateBaseline(Guid projectId, CreateBaselineRequest request)
    {
        var result = await _mediator.Send(new CreateGanttBaselineCommand(projectId, request.Name));
        return Ok(result);
    }

    [HttpGet("baselines")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<IReadOnlyList<GanttBaselineDto>>> ListBaselines(Guid projectId)
    {
        var result = await _mediator.Send(new ListGanttBaselinesQuery(projectId));
        return Ok(result);
    }

    [HttpGet("baselines/{baselineId:guid}")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<GanttBaselineDetailDto>> GetBaseline(Guid projectId, Guid baselineId)
    {
        var result = await _mediator.Send(new GetGanttBaselineQuery(projectId, baselineId));
        return Ok(result);
    }

    [HttpGet("export")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<IActionResult> Export(Guid projectId, [FromQuery] string format = "csv")
    {
        var csv = await _mediator.Send(new ExportGanttCsvQuery(projectId));
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"gantt-export-{projectId}.csv");
    }
}
