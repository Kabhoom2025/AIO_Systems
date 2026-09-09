using LinkShield.Application.Interfaces;

namespace LinkShield.Tests.TestDoubles;

public class FakeEmailSender : IEmailSender
{
    public List<(string ToEmail, string Subject, string Body)> SentEmails { get; } = new();

    public Task SendAsync(string toEmail, string subject, string body)
    {
        SentEmails.Add((toEmail, subject, body));
        return Task.CompletedTask;
    }
}
