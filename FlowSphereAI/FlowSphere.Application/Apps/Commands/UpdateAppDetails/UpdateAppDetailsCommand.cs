using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.UpdateAppDetails;

public record UpdateAppDetailsCommand(int AppId, string Name, string? Description, string? Icon)
    : IRequest<Result>, IAppScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}
