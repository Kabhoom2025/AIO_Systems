using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LinkShield.Infrastructure.Notifications;

/// <summary>
/// Dev/scaffold stand-in for a real SMTP/transactional-email provider — writes the email to
/// the log instead of sending it, so verification/reset tokens are visible during local
/// development without needing a mail server configured. Swap this registration for a real
/// IEmailSender implementation (SendGrid/SES/SMTP) before any non-local deployment.
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation("[DEV EMAIL] To: {ToEmail} | Subject: {Subject}\n{Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }
}
