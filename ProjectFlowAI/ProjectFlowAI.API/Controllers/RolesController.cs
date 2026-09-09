using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Roles;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator) => _mediator = mediator;

    public record CreateRoleRequest(Guid? OrganizationId, string Name, IReadOnlyList<string> PermissionKeys);
    public record AssignRoleRequest(Guid UserId, Guid RoleId, Guid? OrganizationId);
    public record RevokeRoleRequest(Guid UserId, Guid RoleId);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.RolesView)]
    public async Task<ActionResult<PagedResult<RoleDto>>> List(
        [FromQuery] Guid? organizationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "asc")
    {
        var result = await _mediator.Send(new ListRolesQuery(organizationId, page, pageSize, sortBy, sortDir));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.RolesManage)]
    public async Task<ActionResult<RoleDto>> Create(CreateRoleRequest request)
    {
        var result = await _mediator.Send(new CreateRoleCommand(request.OrganizationId, request.Name, request.PermissionKeys));
        return Ok(result);
    }

    [HttpPost("assign")]
    [Authorize(Policy = PermissionCatalog.RolesManage)]
    public async Task<IActionResult> Assign(AssignRoleRequest request)
    {
        await _mediator.Send(new AssignRoleCommand(request.UserId, request.RoleId, request.OrganizationId));
        return NoContent();
    }

    [HttpPost("revoke")]
    [Authorize(Policy = PermissionCatalog.RolesManage)]
    public async Task<IActionResult> Revoke(RevokeRoleRequest request)
    {
        await _mediator.Send(new RevokeRoleCommand(request.UserId, request.RoleId));
        return NoContent();
    }
}

[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.RolesView)]
    public async Task<ActionResult<IReadOnlyList<PermissionDto>>> List()
    {
        var result = await _mediator.Send(new ListPermissionsQuery());
        return Ok(result);
    }
}
