using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions);

    /// <summary>Returns the raw (unhashed) 256-bit refresh token that is handed to the client.</summary>
    string GenerateRefreshToken();

    /// <summary>SHA256 of the raw token — the only form ever persisted.</summary>
    string HashToken(string rawToken);

    bool ValidateRefreshToken(string rawToken, string storedHash);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string secret, string email, string issuer);
    bool ValidateCode(string secret, string code);
}

public record OAuthUserInfo(string Email, string Name, string ProviderId);

public interface IOAuthVerifier
{
    Task<OAuthUserInfo> VerifyGoogleIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
    Task<OAuthUserInfo> VerifyMicrosoftAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);
}

/// <summary>Thrown by IOAuthVerifier implementations when verification fails; the API layer maps this to 401.</summary>
public class OAuthVerificationException : Exception
{
    public OAuthVerificationException(string message) : base(message) { }
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? OrganizationId { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    string? IpAddress { get; }
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

/// <summary>Mirrors NovaERP's IAuditLogWriter — persists one AuditLog row per mutating request.</summary>
public interface IAuditLogWriter
{
    Task WriteAsync(Guid? organizationId, Guid? userId, string action, string entityType,
        string? entityId, string? metadataJson, string? ipAddress, CancellationToken cancellationToken = default);
}

public record StoredFile(string RelativePath, long SizeBytes);

/// <summary>Simple local-disk file storage for WorkItem attachments (App_Data/attachments under
/// ProjectFlowAI.API). Deliberately not S3/Azure Blob-backed — a local-file convention is enough
/// for this phase, mirroring the equivalent NovaERP local storage helper.</summary>
public interface IFileStorageService
{
    Task<StoredFile> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
    Stream OpenRead(string relativePath);
    void Delete(string relativePath);
    string GetContentType(string fileName);
}

// ---------------------------------------------------------------------------
// Phase 4: Chat / Notifications — outbound channel senders + realtime push
// ---------------------------------------------------------------------------

/// <summary>A single configurable-webhook outbound notification channel (Slack/Teams). Every
/// implementation must genuinely no-op-and-log when its org has no webhook URL configured —
/// exactly like SmtpEmailSender degrades when Smtp:Host is unconfigured — never throw for that case.</summary>
public interface ISlackNotifier
{
    Task SendAsync(string webhookUrl, string title, string body, CancellationToken cancellationToken = default);
}

public interface ITeamsNotifier
{
    Task SendAsync(string webhookUrl, string title, string body, CancellationToken cancellationToken = default);
}

/// <summary>Generic HTTP SMS provider integration (Twilio-shaped-but-provider-agnostic): POSTs to a
/// configurable endpoint with an API key. No-ops-and-logs when unconfigured.</summary>
public interface ISmsNotifier
{
    Task SendAsync(string providerUrl, string apiKey, string toPhoneNumber, string body, CancellationToken cancellationToken = default);
}

/// <summary>Implemented in the API layer (backed by IHubContext&lt;ChatHub&gt;) so Application-layer
/// command handlers can push realtime chat events without depending on ASP.NET Core SignalR types.</summary>
public interface IChatRealtimeNotifier
{
    Task MessageReceivedAsync(Guid? channelId, Guid? directConversationId, object message, CancellationToken cancellationToken = default);
    Task MessageDeletedAsync(Guid? channelId, Guid? directConversationId, Guid messageId, CancellationToken cancellationToken = default);
    Task ReactionChangedAsync(Guid? channelId, Guid? directConversationId, Guid messageId, object reactions, CancellationToken cancellationToken = default);
}

/// <summary>Implemented in the API layer (backed by IHubContext&lt;NotificationsHub&gt;).</summary>
public interface INotificationRealtimeNotifier
{
    Task NotificationReceivedAsync(Guid userId, object notification, CancellationToken cancellationToken = default);
}

/// <summary>Creates a Notification row, pushes it over the NotificationsHub, and (respecting each
/// user's NotificationPreference) fans out to Email/Slack/Teams/SMS. Called from other features
/// (work-item comment mentions, assignment changes, sprint start) — never invoked directly by a
/// controller.</summary>
public interface INotificationDispatcher
{
    Task DispatchAsync(Guid userId, NotificationType type, string title, string body,
        string? linkUrl = null, CancellationToken cancellationToken = default);
}

// ---------------------------------------------------------------------------
// Phase 6: Workflow Automation — Hangfire-backed Scheduled trigger
// ---------------------------------------------------------------------------

/// <summary>Registers/removes Hangfire recurring jobs for Scheduled-trigger WorkflowDefinitions.
/// Implemented in the Infrastructure layer so the Application layer never depends on the Hangfire
/// package directly — mirrors how INotificationDispatcher keeps SignalR out of Application.</summary>
public interface IWorkflowScheduler
{
    void RegisterOrUpdate(Guid workflowDefinitionId, string cronExpression);
    void Remove(Guid workflowDefinitionId);
}
