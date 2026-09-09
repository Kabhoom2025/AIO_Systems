namespace FlowSphere.Application.Interfaces;

public record SmsSendResult(bool Sent, string? ErrorMessage);

/// <summary>Sends a real OTP SMS via the org's built-in Twilio ConnectorCredential (same
/// AccountSid/AuthToken/FromNumber rows the workflow Twilio node reads). Independent of
/// FlowSphere.Execution for the same reason as IAppNotificationEmailSender.</summary>
public interface IAppOtpSmsSender
{
    Task<SmsSendResult> SendAsync(int organizationId, string toPhoneNumber, string body, CancellationToken cancellationToken);
}
