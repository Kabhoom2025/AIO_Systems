using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Milestones;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/milestones")]
[Authorize]
public class MilestonesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MilestonesController(IMediator mediator) => _mediator = mediator;

    public record CreateMilestoneRequest(Guid ProjectId, string Name, string? Description, DateTime? DueDate);
    public record UpdateMilestoneRequest(string Name, string? Description, DateTime? DueDate, MilestoneStatus Status);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.ProjectsView)]
    public async Task<ActionResult<PagedResult<MilestoneDto>>> List(
        [FromQuery] Guid projectId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "asc")
    {
        var result = await _mediator.Send(new ListMilestonesQuery(projectId, page, pageSize, sortBy, sortDir));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.MilestonesManage)]
    public async Task<ActionResult<MilestoneDto>> Create(CreateMilestoneRequest request)
    {
        var result = await _mediator.Send(new CreateMilestoneCommand(request.ProjectId, request.Name, request.Description, request.DueDate));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.MilestonesManage)]
    public async Task<ActionResult<MilestoneDto>> Update(Guid id, UpdateMilestoneRequest request)
    {
        var result = await _mediator.Send(new UpdateMilestoneCommand(id, request.Name, request.Description, request.DueDate, request.Status));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.MilestonesManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteMilestoneCommand(id));
        return NoContent();
    }
}
