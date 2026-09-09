using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Dashboard;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Controllers;

/// <summary>Always scoped to the calling user (ICurrentUserService) — there is no organization-wide
/// or other-user variant of this endpoint, so it needs no permission beyond [Authorize].</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var result = await _mediator.Send(new GetDashboardQuery(CurrentUserId));
        return Ok(result);
    }
}
