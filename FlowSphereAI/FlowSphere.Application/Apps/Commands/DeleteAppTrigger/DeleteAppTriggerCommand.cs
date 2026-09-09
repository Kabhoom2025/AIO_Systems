using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.DeleteAppTrigger;

public record DeleteAppTriggerCommand(int SourceAppId, int TriggerId) : IRequest<Result>, IAppScopedRequest
{
    public int AppId => SourceAppId;
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}
