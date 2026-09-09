using System.Net;
using System.Net.Mail;
using FlowSphere.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Infrastructure.Email;

/// <summary>Mirrors FlowSphere.Execution's SmtpConnector/EmailSmtpNodeExecutor pair (same
/// built-in "Smtp" Connector row, same ConnectorCredential keys: Host/Port/Username/Password) but
/// lives in Infrastructure so app-level notification rules don't need a dependency on the
/// Execution project.</summary>
public class AppNotificationEmailSender : IAppNotificationEmailSender
{
    private readonly IApplicationDbContext _db;
    private readonly IConnectorCredentialStore _credentialStore;

    public AppNotificationEmailSender(IApplicationDbContext db, IConnectorCredentialStore credentialStore)
    {
        _db = db;
        _credentialStore = credentialStore;
    }

    public async Task<EmailSendResult> SendAsync(
        int organizationId, IReadOnlyCollection<string> to, IReadOnlyCollection<string> cc, string subject, string body, CancellationToken cancellationToken)
    {
        if (to.Count == 0)
        {
            return new EmailSendResult(false, "No recipients resolved for this notification rule.");
        }

        var smtpConnectorId = await _db.Connectors
            .Where(c => c.Type == "Smtp" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (smtpConnectorId == 0)
        {
            return new EmailSendResult(false, "No built-in Smtp connector is configured.");
        }

        var host = await _credentialStore.GetAsync(smtpConnectorId, "Host", organizationId, cancellationToken);
        if (string.IsNullOrWhiteSpace(host))
        {
            return new EmailSendResult(false, "This organization has no SMTP credentials configured - notification email was not sent.");
        }

        var portRaw = await _credentialStore.GetAsync(smtpConnectorId, "Port", organizationId, cancellationToken);
        var username = await _credentialStore.GetAsync(smtpConnectorId, "Username", organizationId, cancellationToken);
        var password = await _credentialStore.GetAsync(smtpConnectorId, "Password", organizationId, cancellationToken);
        var port = int.TryParse(portRaw, out var p) ? p : 587;

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = string.IsNullOrEmpty(username) ? null : new NetworkCredential(username, password),
            };

            using var message = new MailMessage
            {
                From = new MailAddress(username ?? "flowsphere@localhost"),
                Subject = subject,
                Body = body,
            };
            foreach (var address in to)
            {
                message.To.Add(address);
            }
            foreach (var address in cc)
            {
                message.CC.Add(address);
            }

            await client.SendMailAsync(message, cancellationToken);
            return new EmailSendResult(true, null);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException or FormatException)
        {
            return new EmailSendResult(false, $"SMTP send error: {ex.Message}");
        }
    }
}
