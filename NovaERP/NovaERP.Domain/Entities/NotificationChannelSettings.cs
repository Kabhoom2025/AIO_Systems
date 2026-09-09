namespace NovaERP.Domain.Entities;

/// <summary>One row per Organization holding optional provider credentials for outbound
/// Email/SMS/Push delivery. Every provider field is nullable — an org that hasn't configured any
/// channel yet is the normal default state, not an error state (see IEmailSender/ISmsSender/
/// IPushSender, which return a "NotConfigured" result rather than throwing in that case).</summary>
public class NotificationChannelSettings : BaseEntity
{
    public int OrganizationId { get; set; }

    // Email (SMTP)
    public string? SmtpHost      { get; set; }
    public int?    SmtpPort      { get; set; }
    public string? SmtpUsername  { get; set; }
    public string? SmtpPassword  { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName  { get; set; }
    public bool    SmtpUseSsl    { get; set; } = true;

    // SMS — generic HTTP gateway adapter (point at Twilio/Msg91/etc.)
    public string? SmsApiUrl   { get; set; }
    public string? SmsApiKey   { get; set; }
    public string? SmsSenderId { get; set; }

    // Push — generic HTTP gateway adapter (FCM-legacy-shaped body)
    public string? PushApiUrl     { get; set; }
    public string? PushServerKey  { get; set; }

    public Organization Organization { get; set; } = null!;
}
