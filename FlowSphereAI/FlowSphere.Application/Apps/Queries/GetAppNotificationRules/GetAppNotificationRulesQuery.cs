using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetAppNotificationRules;

public record GetAppNotificationRulesQuery(int AppId) : IRequest<Result<List<AppNotificationRuleDto>>>;

public record AppNotificationRuleDto(
    int Id, string Name, AppNotificationEvent Event, AppNotificationLevel Level, string? StepKey, string? ActionKey,
    AppNotificationChannel Channel, string? SenderEmail, AppNotificationRecipientType RecipientType,
    List<int> RecipientRoleIds, List<int> RecipientUserIds, string? RecipientDataFieldKey, List<int> CcUserIds,
    string Subject, string Body, string ConditionsJson, bool SendAttachments, bool SendReports, bool IsEnabled);
