using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.TimeTracking;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/timesheets")]
[Authorize]
public class TimesheetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public TimesheetsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    // userId is optional and defaults to the caller — org admins/PMs can look up others via TasksView,
    // deliberately not gated behind a separate per-user ACL per the spec's "don't overbuild" guidance.
    [HttpGet]
    [Authorize(Policy = PermissionCatalog.TasksView)]
    public async Task<ActionResult<TimesheetDto>> Get([FromQuery] Guid? userId, [FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        var result = await _mediator.Send(new GetTimesheetQuery(userId ?? CurrentUserId, from, to));
        return Ok(result);
    }
}
