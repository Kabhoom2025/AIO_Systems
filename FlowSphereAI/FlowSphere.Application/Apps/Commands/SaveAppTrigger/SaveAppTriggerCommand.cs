using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.SaveAppTrigger;

/// <summary>Id null creates a new trigger; a non-null Id updates the existing one (must already
/// belong to SourceAppId). SourceAppId doubles as the AppId for IAppScopedRequest's permission
/// check - editing a trigger requires apps.write on the source app.</summary>
public record SaveAppTriggerCommand(
    int? Id, int SourceAppId, string Name, int DestinationAppId, AppTriggerActionType ActionType, string FieldMappingJson, bool IsEnabled)
    : IRequest<Result<int>>, IAppScopedRequest
{
    public int AppId => SourceAppId;
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}
