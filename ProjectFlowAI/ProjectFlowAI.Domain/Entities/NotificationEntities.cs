namespace ProjectFlowAI.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; } = NotificationType.General;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

/// <summary>Per-user, per-channel opt-out. Absence of a row for a given channel means "enabled"
/// (see GetNotificationPreferencesQueryHandler, which seeds the missing defaults on read rather
/// than erroring).</summary>
public class NotificationPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsEnabled { get; set; } = true;

    public User? User { get; set; }
}

/// <summary>Org-level webhook/provider config for Slack/Teams/SMS fan-out. Empty by default —
/// NotificationDispatcher's channel senders no-op-and-log when their URL is unset, exactly like
/// SmtpEmailSender degrades when Smtp:Host is unconfigured.</summary>
public class OrganizationIntegrationSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string? SlackWebhookUrl { get; set; }
    public string? TeamsWebhookUrl { get; set; }
    public string? SmsProviderUrl { get; set; }
    public string? SmsProviderApiKey { get; set; }

    public Organization? Organization { get; set; }
}
