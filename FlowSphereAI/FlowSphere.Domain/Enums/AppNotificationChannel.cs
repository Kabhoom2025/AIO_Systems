namespace FlowSphere.Domain.Enums;

/// <summary>Only Email actually dispatches (via IAppNotificationEmailSender). Sms/WhatsApp are
/// selectable and persisted but never sent - no SMS/WhatsApp provider exists for app-level
/// notifications (Twilio SMS exists only as a workflow-connector node) - flagged "not dispatched"
/// client-side rather than faking delivery.</summary>
public enum AppNotificationChannel
{
    Email,
    Sms,
    WhatsApp
}
