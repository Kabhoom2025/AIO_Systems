using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Labels;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/labels")]
[Authorize]
public class LabelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelsController(IMediator mediator) => _mediator = mediator;

    public record CreateLabelRequest(Guid ProjectId, string Name, string ColorHex);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.ProjectsView)]
    public async Task<ActionResult<IReadOnlyList<LabelDto>>> List([FromQuery] Guid projectId)
    {
        var result = await _mediator.Send(new ListLabelsQuery(projectId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.ProjectsManage)]
    public async Task<ActionResult<LabelDto>> Create(CreateLabelRequest request)
    {
        var result = await _mediator.Send(new CreateLabelCommand(request.ProjectId, request.Name, request.ColorHex));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.ProjectsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteLabelCommand(id));
        return NoContent();
    }
}
