using System.Net;
using System.Net.Mail;
using System.Text.Json;
using FlowSphere.Application.Interfaces;

namespace FlowSphere.Execution.Connectors;

/// <summary>Node config shape: { "to": "a@b.com", "subject": "...", "body": "..." }. SMTP
/// server/credentials come from ConnectorCredential rows (keys: Host, Port, Username, Password,
/// EnableSsl) scoped to the calling organization - resolved by the caller (EmailSmtpNodeExecutor)
/// and passed in via ConnectorInvocation.ConfigJson merged with the node config, so this
/// connector stays a pure "given full config, send the email" implementation.</summary>
public class SmtpConnector : IConnector
{
    public string Type => "Smtp";

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        string? Get(string name) => root.TryGetProperty(name, out var el) ? el.GetString() : null;

        var host = Get("smtpHost");
        var to = Get("to");
        var subject = Get("subject") ?? "";
        var body = Get("body") ?? "";
        var username = Get("smtpUsername");
        var password = Get("smtpPassword");
        var port = root.TryGetProperty("smtpPort", out var portEl) && portEl.TryGetInt32(out var p) ? p : 587;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(to))
        {
            return ConnectorResult.Fail("Email node config is missing 'to' or SMTP host credential.");
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = string.IsNullOrEmpty(username) ? null : new NetworkCredential(username, password),
            };

            using var message = new MailMessage(username ?? "flowsphere@localhost", to, subject, body);
            await client.SendMailAsync(message, cancellationToken);

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { sent = true, to }));
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException)
        {
            return ConnectorResult.Fail($"SMTP send error: {ex.Message}");
        }
    }
}
