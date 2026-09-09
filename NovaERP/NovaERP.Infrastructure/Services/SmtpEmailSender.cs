using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

/// <summary>Real SmtpClient-based sender. Mirrors the FoodOrder EmailService pattern: reads SMTP
/// config from the org's NotificationChannelSettings row and, when unconfigured, returns a clean
/// "NotConfigured" result rather than throwing so callers (dispatcher, test-send endpoint) never
/// crash on a missing configuration.</summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly NovaErpDbContext _ctx;

    public SmtpEmailSender(NovaErpDbContext ctx) => _ctx = ctx;

    public async Task<SendResultDto> SendAsync(int organizationId, string toEmail, string subject, string body)
    {
        var settings = await _ctx.NotificationChannelSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId);

        if (settings == null || string.IsNullOrWhiteSpace(settings.SmtpHost) || string.IsNullOrWhiteSpace(settings.SmtpFromEmail))
            return new SendResultDto(false, "NotConfigured", "SMTP is not configured for this organization.");

        try
        {
            var port = settings.SmtpPort is > 0 ? settings.SmtpPort.Value : 587;
            var fromName = string.IsNullOrWhiteSpace(settings.SmtpFromName) ? "NovaERP" : settings.SmtpFromName;

            using var smtp = new SmtpClient(settings.SmtpHost, port)
            {
                Credentials = string.IsNullOrWhiteSpace(settings.SmtpUsername)
                    ? null
                    : new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
                EnableSsl = settings.SmtpUseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var msg = new MailMessage
            {
                From = new MailAddress(settings.SmtpFromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            msg.To.Add(new MailAddress(toEmail));

            await smtp.SendMailAsync(msg);
            return new SendResultDto(true, "Sent", null);
        }
        catch (Exception ex)
        {
            return new SendResultDto(false, "Failed", ex.Message);
        }
    }
}
