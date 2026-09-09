using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Sprints;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/sprints")]
[Authorize]
public class SprintsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public SprintsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // "Who is performing this action" always comes from the authenticated JWT, never a client-supplied field.
    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    public record CreateSprintRequest(Guid ProjectId, string Name, string? Goal, DateTime StartDate, DateTime EndDate);
    public record UpdateSprintRequest(string Name, string? Goal, DateTime StartDate, DateTime EndDate);
    public record AddRetrospectiveNoteRequest(RetrospectiveCategory Category, string Text);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<PagedResult<SprintDto>>> List(
        [FromQuery] Guid? projectId, [FromQuery] SprintStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "asc")
    {
        var result = await _mediator.Send(new ListSprintsQuery(projectId, status, page, pageSize, sortBy, sortDir));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.SprintsManage)]
    public async Task<ActionResult<SprintDto>> Create(CreateSprintRequest request)
    {
        var result = await _mediator.Send(new CreateSprintCommand(request.ProjectId, request.Name, request.Goal, request.StartDate, request.EndDate));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.SprintsManage)]
    public async Task<ActionResult<SprintDto>> Update(Guid id, UpdateSprintRequest request)
    {
        var result = await _mediator.Send(new UpdateSprintCommand(id, request.Name, request.Goal, request.StartDate, request.EndDate));
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Policy = PermissionCatalog.SprintsManage)]
    public async Task<ActionResult<SprintDto>> Start(Guid id)
    {
        var result = await _mediator.Send(new StartSprintCommand(id));
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = PermissionCatalog.SprintsManage)]
    public async Task<ActionResult<SprintDto>> Complete(Guid id)
    {
        var result = await _mediator.Send(new CompleteSprintCommand(id));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.SprintsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteSprintCommand(id));
        return NoContent();
    }

    [HttpGet("{id:guid}/board")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<SprintBoardDto>> Board(Guid id)
    {
        var result = await _mediator.Send(new GetSprintBoardQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:guid}/burndown")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<BurndownDto>> Burndown(Guid id)
    {
        var result = await _mediator.Send(new GetSprintBurndownQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:guid}/burnup")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<BurnupDto>> Burnup(Guid id)
    {
        var result = await _mediator.Send(new GetSprintBurnupQuery(id));
        return Ok(result);
    }

    // --- Retrospective ---

    [HttpGet("{id:guid}/retrospective")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<RetrospectiveDto>> Retrospective(Guid id)
    {
        var result = await _mediator.Send(new GetSprintRetrospectiveQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:guid}/retrospective/notes")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<RetrospectiveNoteDto>> AddRetrospectiveNote(Guid id, AddRetrospectiveNoteRequest request)
    {
        var result = await _mediator.Send(new AddRetrospectiveNoteCommand(id, request.Category, request.Text, CurrentUserId));
        return Ok(result);
    }

    [HttpDelete("/api/retrospective-notes/{noteId:guid}")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<IActionResult> DeleteRetrospectiveNote(Guid noteId)
    {
        await _mediator.Send(new DeleteRetrospectiveNoteCommand(noteId));
        return NoContent();
    }
}
