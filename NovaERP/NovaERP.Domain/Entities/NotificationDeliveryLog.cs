namespace NovaERP.Domain.Entities;

/// <summary>Audit trail of one outbound delivery attempt (Email/Sms/Push) tied to an in-app
/// Notification. Written by IMultiChannelNotificationDispatcher for every channel it attempts,
/// regardless of outcome, so failed deliveries are visible without throwing.</summary>
public class NotificationDeliveryLog : BaseEntity
{
    public int      NotificationId { get; set; }
    public string   Channel        { get; set; } = string.Empty; // "Email" | "Sms" | "Push"
    public string   Status         { get; set; } = string.Empty; // "Sent" | "Failed" | "NotConfigured"
    public string?  Error          { get; set; }
    public DateTime SentDate       { get; set; } = DateTime.UtcNow;

    public Notification Notification { get; set; } = null!;
}
