using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Queries.GetUserRoleAssignments;

public record GetUserRoleAssignmentsQuery(int WorkspaceId, int? WorkspaceRoleId) : IRequest<Result<IReadOnlyList<UserRoleAssignmentDto>>>;

public record AssignedRoleDto(int WorkspaceRoleId, string Name);

public record UserRoleAssignmentDto(int UserId, string UserName, string Email, List<AssignedRoleDto> Roles);
