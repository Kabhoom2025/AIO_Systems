using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Reports;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

/// <summary>Phase 5: read-only computed reports/analytics over existing WorkItem/Sprint/TimeLog
/// data. Every action requires the "reports.view" permission (seeded since Phase 1/2 as a
/// placeholder, implemented for real here).</summary>
[ApiController]
[Route("api/reports")]
[Authorize(Policy = PermissionCatalog.ReportsView)]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("executive")]
    public async Task<ActionResult<ExecutiveDashboardDto>> Executive([FromQuery] Guid organizationId)
    {
        var result = await _mediator.Send(new GetExecutiveDashboardQuery(organizationId));
        return Ok(result);
    }

    [HttpGet("sprint/{sprintId:guid}")]
    public async Task<ActionResult<SprintDashboardDto>> Sprint(Guid sprintId)
    {
        var result = await _mediator.Send(new GetSprintDashboardQuery(sprintId));
        return Ok(result);
    }

    [HttpGet("cycle-time")]
    public async Task<ActionResult<CycleTimeReportDto>> CycleTime([FromQuery] Guid projectId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _mediator.Send(new GetCycleTimeReportQuery(projectId, from, to));
        return Ok(result);
    }

    [HttpGet("project-health")]
    public async Task<ActionResult<ProjectHealthDto>> ProjectHealth([FromQuery] Guid projectId)
    {
        var result = await _mediator.Send(new GetProjectHealthQuery(projectId));
        return Ok(result);
    }

    [HttpGet("productivity")]
    public async Task<ActionResult<ProductivityReportDto>> Productivity(
        [FromQuery] Guid projectId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] ProductivityBucket bucket = ProductivityBucket.Week)
    {
        var result = await _mediator.Send(new GetProductivityReportQuery(projectId, from, to, bucket));
        return Ok(result);
    }

    [HttpGet("resource-utilization")]
    public async Task<ActionResult<ResourceUtilizationReportDto>> ResourceUtilization(
        [FromQuery] Guid organizationId, [FromQuery] Guid? projectId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _mediator.Send(new GetResourceUtilizationReportQuery(organizationId, projectId, from, to));
        return Ok(result);
    }

    [HttpGet("cost-analysis")]
    public async Task<ActionResult<CostAnalysisReportDto>> CostAnalysis(
        [FromQuery] Guid projectId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _mediator.Send(new GetCostAnalysisReportQuery(projectId, from, to));
        return Ok(result);
    }
}
