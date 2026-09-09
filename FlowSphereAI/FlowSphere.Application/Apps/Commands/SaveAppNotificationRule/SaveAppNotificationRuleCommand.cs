using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.SaveAppNotificationRule;

/// <summary>Id null creates a new rule; a non-null Id updates the existing one (must already
/// belong to AppId). Same "one command, create-or-update" shape used elsewhere for small
/// per-record settings in this module.</summary>
public record SaveAppNotificationRuleCommand(
    int? Id, int AppId, string Name, AppNotificationEvent Event, AppNotificationLevel Level, string? StepKey, string? ActionKey,
    AppNotificationChannel Channel, string? SenderEmail, AppNotificationRecipientType RecipientType,
    List<int> RecipientRoleIds, List<int> RecipientUserIds, string? RecipientDataFieldKey, List<int> CcUserIds,
    string Subject, string Body, string ConditionsJson, bool SendAttachments, bool SendReports, bool IsEnabled)
    : IRequest<Result<int>>, IAppScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}
