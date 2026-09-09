using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.RollbackApp;

public record RollbackAppCommand(int AppId) : IRequest<Result>, IAppScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}
