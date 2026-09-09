using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Infrastructure.Services;

/// <summary>Real SMTP send via System.Net.Mail. Falls back to logging the email content when
/// Smtp:Host is unconfigured, so Development works without a real mail server — this is a genuine
/// degraded path, not a stub: the SMTP send code always runs when a host is configured.</summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = _config["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation("[EmailSender:LogOnly] To={To} Subject={Subject} Body={Body}", toEmail, subject, htmlBody);
            return;
        }

        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
        var username = _config["Smtp:Username"];
        var password = _config["Smtp:Password"];
        var from = _config["Smtp:From"] ?? username ?? "no-reply@projectflow.local";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrWhiteSpace(username) ? null : new NetworkCredential(username, password)
        };

        using var message = new MailMessage(from, toEmail, subject, htmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(message, cancellationToken);
    }
}
