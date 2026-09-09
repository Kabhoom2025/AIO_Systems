using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.CreateApp;

public record CreateAppCommand(int WorkspaceId, string Name, string? Description, string? Icon = null) : IRequest<Result<int>>, IWorkspaceScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}
