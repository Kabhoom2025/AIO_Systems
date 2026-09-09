using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

/// <summary>Real SMTP-based email delivery. Reads NotificationChannelSettings for the given org;
/// returns a "NotConfigured" result (never throws) when SmtpHost/SmtpFromEmail are blank.</summary>
public interface IEmailSender
{
    Task<SendResultDto> SendAsync(int organizationId, string toEmail, string subject, string body);
}
