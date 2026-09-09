using System.Collections.Concurrent;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Tests.TestDoubles;

/// <summary>Test-only IEmailSender that records every "sent" email body instead of talking to SMTP,
/// so integration tests can pull the verification/reset code out of the email body they'd
/// otherwise have no way to observe (the raw token is only ever persisted hashed).</summary>
public class CapturingEmailSender : IEmailSender
{
    public static readonly ConcurrentBag<(string To, string Subject, string Body)> Sent = new();

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        Sent.Add((toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }
}
