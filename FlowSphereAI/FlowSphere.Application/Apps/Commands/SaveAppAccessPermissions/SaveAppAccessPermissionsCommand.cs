using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.SaveAppAccessPermissions;

public record SaveAppAccessPermissionsCommand(int AppId, string AccessPermissionsJson) : IRequest<Result>, IAppScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}
