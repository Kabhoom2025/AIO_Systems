using FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.AssignWorkspaceRole;
using FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.CreateWorkspaceRole;
using FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.DeleteWorkspaceRole;
using FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.RemoveWorkspaceRoleAssignment;
using FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.UpdateWorkspaceRole;
using FlowSphere.Application.Workspaces.WorkspaceRoles.Queries.GetUserRoleAssignments;
using FlowSphere.Application.Workspaces.WorkspaceRoles.Queries.GetWorkspaceRoles;
using FlowSphere.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

/// <summary>Creating/editing a WorkspaceRole (and its assignment) is itself a workspace-admin
/// action, gated by the existing global workspaces.write/read policies - it doesn't itself need
/// the new per-workspace enforcement (that's for Apps/Tables write actions).</summary>
[Authorize]
[Route("api/workspace-roles")]
public class WorkspaceRolesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCatalog.WorkspacesRead)]
    public async Task<IActionResult> GetList([FromQuery] int? workspaceId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetWorkspaceRolesQuery(workspaceId), cancellationToken);
        return FromResult(result);
    }

    public record CreateWorkspaceRoleRequest(int WorkspaceId, string Name, List<string> Permissions);

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> Create(CreateWorkspaceRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new CreateWorkspaceRoleCommand(request.WorkspaceId, request.Name, request.Permissions), cancellationToken);
        return FromResult(result, id => Ok(new { id }));
    }

    public record UpdateWorkspaceRoleRequest(string Name, List<string> Permissions);

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> Update(int id, UpdateWorkspaceRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateWorkspaceRoleCommand(id, request.Name, request.Permissions), cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteWorkspaceRoleCommand(id), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("assignments")]
    [Authorize(Policy = PermissionCatalog.WorkspacesRead)]
    public async Task<IActionResult> GetAssignments([FromQuery] int workspaceId, [FromQuery] int? roleId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUserRoleAssignmentsQuery(workspaceId, roleId), cancellationToken);
        return FromResult(result);
    }

    public record AssignRequest(int WorkspaceId, int UserId, int WorkspaceRoleId);

    [HttpPost("assignments")]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> Assign(AssignRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new AssignWorkspaceRoleCommand(request.WorkspaceId, request.UserId, request.WorkspaceRoleId), cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("assignments/{workspaceId:int}/{userId:int}/{workspaceRoleId:int}")]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> RemoveAssignment(int workspaceId, int userId, int workspaceRoleId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RemoveWorkspaceRoleAssignmentCommand(workspaceId, userId, workspaceRoleId), cancellationToken);
        return FromResult(result);
    }
}
