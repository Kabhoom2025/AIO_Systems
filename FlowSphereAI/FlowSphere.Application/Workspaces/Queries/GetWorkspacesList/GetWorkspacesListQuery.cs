using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.Queries.GetWorkspacesList;

public record GetWorkspacesListQuery : IRequest<Result<List<WorkspaceSummaryDto>>>;

public record WorkspaceSummaryDto(
    int Id,
    string Name,
    string? Description,
    int AppCount,
    DateTime CreatedDate,
    List<AdminUserDto> WorkspaceAdmins,
    List<AdminUserDto> DataAdmins);
