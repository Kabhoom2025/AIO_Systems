using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.MoveAppToWorkspace;

/// <summary>Moves every stage-clone sharing this app's SourceGroupId (Dev/QA/UAT/Live) to a new
/// workspace as one unit - moving only the current stage's row would fork "the same app" across
/// two workspaces, which doesn't match how promotion already treats SourceGroupId.</summary>
public record MoveAppToWorkspaceCommand(int AppId, int TargetWorkspaceId) : IRequest<Result>, IAppScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}
