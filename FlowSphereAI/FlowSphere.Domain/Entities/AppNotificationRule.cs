using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>One configured notification rule for an app, evaluated against a submitted
/// AppRecord's DataJson (see AppNotificationDispatcher). Only Event=RecordSubmitted/Custom with
/// Channel=Email actually dispatch today - see AppNotificationEvent/AppNotificationChannel doc
/// comments for the honest scope cut.</summary>
public class AppNotificationRule : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public int AppId { get; set; }
    public string Name { get; set; } = string.Empty;

    public AppNotificationEvent Event { get; set; } = AppNotificationEvent.RecordSubmitted;
    public AppNotificationLevel Level { get; set; } = AppNotificationLevel.AllStepsAndActions;
    public string? StepKey { get; set; }
    public string? ActionKey { get; set; }

    public AppNotificationChannel Channel { get; set; } = AppNotificationChannel.Email;
    public string? SenderEmail { get; set; }

    public AppNotificationRecipientType RecipientType { get; set; } = AppNotificationRecipientType.Users;
    public string RecipientRoleIdsJson { get; set; } = "[]";
    public string RecipientUserIdsJson { get; set; } = "[]";
    public string? RecipientDataFieldKey { get; set; }
    public string CcUserIdsJson { get; set; } = "[]";

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    /// <summary>List of { fieldKey, operator, value } - evaluated the same way ConditionEvaluator
    /// resolves dot-paths in the execution engine, but re-implemented here (AppNotificationDispatcher)
    /// since Application cannot depend on FlowSphere.Execution.</summary>
    public string ConditionsJson { get; set; } = "[]";

    public bool SendAttachments { get; set; }
    public bool SendReports { get; set; }
    public bool IsEnabled { get; set; } = true;
}
