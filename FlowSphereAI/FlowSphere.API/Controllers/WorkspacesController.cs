using FlowSphere.Application.Workspaces.Commands.AssignWorkspaceAdmin;
using FlowSphere.Application.Workspaces.Commands.CopyWorkspace;
using FlowSphere.Application.Workspaces.Commands.CreateWorkspace;
using FlowSphere.Application.Workspaces.Commands.DeleteWorkspace;
using FlowSphere.Application.Workspaces.Commands.RemoveWorkspaceAdmin;
using FlowSphere.Application.Workspaces.Commands.UpdateWorkspace;
using FlowSphere.Application.Workspaces.Queries.GetWorkspaceAdmins;
using FlowSphere.Application.Workspaces.Queries.GetWorkspaceById;
using FlowSphere.Application.Workspaces.Queries.GetWorkspacesList;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/workspaces")]
public class WorkspacesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCatalog.WorkspacesRead)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetWorkspacesListQuery(), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionCatalog.WorkspacesRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetWorkspaceByIdQuery(id), cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> Create(CreateWorkspaceCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, id => CreatedAtAction(nameof(GetById), new { id }, new { id }));
    }

    public record UpdateWorkspaceRequest(string Name, string? Description);

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> Update(int id, UpdateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateWorkspaceCommand(id, request.Name, request.Description), cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteWorkspaceCommand(id), cancellationToken);
        return FromResult(result);
    }

    public record CopyWorkspaceRequest(int SourceWorkspaceId, string NewWorkspaceName, bool CopyApps, bool CopyTables, int? AdminUserId);

    [HttpPost("copy")]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> Copy(CopyWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new CopyWorkspaceCommand(request.SourceWorkspaceId, request.NewWorkspaceName, request.CopyApps, request.CopyTables, request.AdminUserId),
            cancellationToken);
        return FromResult(result, id => CreatedAtAction(nameof(GetById), new { id }, new { id }));
    }

    [HttpGet("{id:int}/admins")]
    [Authorize(Policy = PermissionCatalog.WorkspacesRead)]
    public async Task<IActionResult> GetAdmins(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetWorkspaceAdminsQuery(id), cancellationToken);
        return FromResult(result);
    }

    public record AssignAdminRequest(int UserId, WorkspaceAdminRole Role);

    [HttpPost("{id:int}/admins")]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> AssignAdmin(int id, AssignAdminRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new AssignWorkspaceAdminCommand(id, request.UserId, request.Role), cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:int}/admins/{userId:int}/{role}")]
    [Authorize(Policy = PermissionCatalog.WorkspacesWrite)]
    public async Task<IActionResult> RemoveAdmin(int id, int userId, WorkspaceAdminRole role, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RemoveWorkspaceAdminCommand(id, userId, role), cancellationToken);
        return FromResult(result);
    }
}
