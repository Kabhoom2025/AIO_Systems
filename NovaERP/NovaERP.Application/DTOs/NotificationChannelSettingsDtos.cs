namespace NovaERP.Application.DTOs;

public class NotificationChannelSettingsDto
{
    public int     Id             { get; set; }
    public int     OrganizationId { get; set; }

    public string? SmtpHost      { get; set; }
    public int?    SmtpPort      { get; set; }
    public string? SmtpUsername  { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName  { get; set; }
    public bool    SmtpUseSsl    { get; set; }

    public string? SmsApiUrl   { get; set; }
    public string? SmsSenderId { get; set; }

    public string? PushApiUrl { get; set; }
}

public class UpdateNotificationChannelSettingsDto
{
    public string? SmtpHost      { get; set; }
    public int?    SmtpPort      { get; set; }
    public string? SmtpUsername  { get; set; }
    public string? SmtpPassword  { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName  { get; set; }
    public bool    SmtpUseSsl    { get; set; } = true;

    public string? SmsApiUrl   { get; set; }
    public string? SmsApiKey   { get; set; }
    public string? SmsSenderId { get; set; }

    public string? PushApiUrl    { get; set; }
    public string? PushServerKey { get; set; }
}

/// <summary>Raw connectivity test used by NotificationController.TestSend — bypasses the
/// in-app Notification entirely since it targets an explicit address, not an existing user/record.</summary>
public class TestSendNotificationDto
{
    public string Channel { get; set; } = string.Empty; // "Email" | "Sms" | "Push"
    public string To      { get; set; } = string.Empty; // email address, phone number, or device token
    public string Message { get; set; } = string.Empty;
    public string? Subject { get; set; } // used for Email only; defaults if blank
}

public record SendResultDto(bool Success, string Status, string? Error);
