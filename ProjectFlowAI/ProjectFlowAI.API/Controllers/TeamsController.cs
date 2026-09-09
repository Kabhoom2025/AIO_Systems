using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Teams;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeamsController(IMediator mediator) => _mediator = mediator;

    public record CreateTeamRequest(Guid OrganizationId, Guid? DepartmentId, string Name, string? Description);
    public record UpdateTeamRequest(string Name, string? Description, Guid? DepartmentId);
    public record AddTeamMemberRequest(Guid UserId, string RoleInTeam);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.TeamsView)]
    public async Task<ActionResult<PagedResult<TeamDto>>> List(
        [FromQuery] Guid organizationId, [FromQuery] Guid? departmentId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "asc",
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new ListTeamsQuery(organizationId, departmentId, page, pageSize, sortBy, sortDir, search));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.TeamsView)]
    public async Task<ActionResult<TeamDto>> Get(Guid id)
    {
        var result = await _mediator.Send(new GetTeamQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.TeamsManage)]
    public async Task<ActionResult<TeamDto>> Create(CreateTeamRequest request)
    {
        var result = await _mediator.Send(new CreateTeamCommand(request.OrganizationId, request.DepartmentId, request.Name, request.Description));
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.TeamsManage)]
    public async Task<ActionResult<TeamDto>> Update(Guid id, UpdateTeamRequest request)
    {
        var result = await _mediator.Send(new UpdateTeamCommand(id, request.Name, request.Description, request.DepartmentId));
        return Ok(result);
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Policy = PermissionCatalog.TeamsManage)]
    public async Task<ActionResult<TeamMemberDto>> AddMember(Guid id, AddTeamMemberRequest request)
    {
        var result = await _mediator.Send(new AddTeamMemberCommand(id, request.UserId, request.RoleInTeam));
        return Ok(result);
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [Authorize(Policy = PermissionCatalog.TeamsManage)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        await _mediator.Send(new RemoveTeamMemberCommand(id, userId));
        return NoContent();
    }
}
