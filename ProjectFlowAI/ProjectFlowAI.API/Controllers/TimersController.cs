using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.TimeTracking;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/timers")]
[Authorize]
public class TimersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public TimersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // Actor is always the authenticated caller — never a client-supplied user id.
    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    public record StartTimerRequest(Guid WorkItemId);

    [HttpPost("start")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<ActiveTimerDto>> Start(StartTimerRequest request)
    {
        var result = await _mediator.Send(new StartTimerCommand(request.WorkItemId, CurrentUserId));
        return Ok(result);
    }

    [HttpPost("stop")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<WorkItemTimeLogDto>> Stop()
    {
        var result = await _mediator.Send(new StopTimerCommand(CurrentUserId));
        return Ok(result);
    }

    [HttpGet("active")]
    [Authorize(Policy = PermissionCatalog.TasksView)]
    public async Task<ActionResult<ActiveTimerDto>> Active()
    {
        var result = await _mediator.Send(new GetActiveTimerQuery(CurrentUserId));
        return result == null ? NoContent() : Ok(result);
    }
}
