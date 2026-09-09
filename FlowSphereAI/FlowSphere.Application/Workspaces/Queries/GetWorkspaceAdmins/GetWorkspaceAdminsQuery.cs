using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.Queries.GetWorkspaceAdmins;

public record GetWorkspaceAdminsQuery(int WorkspaceId) : IRequest<Result<WorkspaceAdminsDto>>;

public record WorkspaceAdminsDto(List<AdminUserDto> WorkspaceAdmins, List<AdminUserDto> DataAdmins);
