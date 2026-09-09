using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.DeleteAppNotificationRule;

public record DeleteAppNotificationRuleCommand(int AppId, int RuleId) : IRequest<Result>, IAppScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}
