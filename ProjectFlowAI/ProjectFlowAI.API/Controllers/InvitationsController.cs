using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Invitations;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/invitations")]
[Authorize]
public class InvitationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationsController(IMediator mediator) => _mediator = mediator;

    public record InviteUserRequest(Guid OrganizationId, string Email, Guid RoleId);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.UsersInvite)]
    public async Task<ActionResult<PagedResult<InvitationDto>>> List(
        [FromQuery] Guid organizationId, [FromQuery] InvitationStatus? status, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "asc")
    {
        var result = await _mediator.Send(new ListInvitationsQuery(organizationId, status, page, pageSize, sortBy, sortDir));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.UsersInvite)]
    public async Task<ActionResult<InvitationDto>> Invite(InviteUserRequest request)
    {
        var invitedBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new InviteUserCommand(request.OrganizationId, request.Email, request.RoleId, invitedBy));
        return Ok(result);
    }

    [HttpPost("{id:guid}/revoke")]
    [Authorize(Policy = PermissionCatalog.UsersInvite)]
    public async Task<IActionResult> Revoke(Guid id)
    {
        await _mediator.Send(new RevokeInvitationCommand(id));
        return NoContent();
    }
}
