namespace FlowSphere.Application.Interfaces;

public record EmailSendResult(bool Sent, string? ErrorMessage);

/// <summary>Sends a real email on behalf of an app-level notification rule, reusing the org's
/// built-in Smtp ConnectorCredential (the same Host/Port/Username/Password rows the workflow
/// EmailSmtp node reads) rather than duplicating a separate credential store. Deliberately
/// independent of FlowSphere.Execution (which depends on Application, not the reverse) - the
/// SMTP send itself is small enough to reimplement here rather than take on that dependency.</summary>
public interface IAppNotificationEmailSender
{
    Task<EmailSendResult> SendAsync(int organizationId, IReadOnlyCollection<string> to, IReadOnlyCollection<string> cc, string subject, string body, CancellationToken cancellationToken);
}
