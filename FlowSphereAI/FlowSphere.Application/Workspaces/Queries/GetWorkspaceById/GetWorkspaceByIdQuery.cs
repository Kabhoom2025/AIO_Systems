using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.Queries.GetWorkspaceById;

public record GetWorkspaceByIdQuery(int Id) : IRequest<Result<WorkspaceDetailDto>>;

public record WorkspaceDetailDto(int Id, string Name, string? Description, DateTime CreatedDate);
