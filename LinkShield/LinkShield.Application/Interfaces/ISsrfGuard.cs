using System.Net;

namespace LinkShield.Application.Interfaces;

public record SsrfCheckResult(bool IsAllowed, string? Reason, IReadOnlyList<IPAddress> ResolvedAddresses)
{
    public static SsrfCheckResult Allowed(IReadOnlyList<IPAddress> addresses) => new(true, null, addresses);
    public static SsrfCheckResult Denied(string reason) => new(false, reason, []);
}

/// <summary>
/// The single gate every outbound call this platform makes against a user-influenced hostname
/// must pass through — DNS resolution result validation, redirect-hop targets, TLS connection
/// targets. See docs/security.md. Resolves the host and rejects it if any candidate address is
/// private/loopback/link-local/multicast, or the host itself looks internal
/// (.local/.internal/single-label). Does NOT itself open a connection — see
/// <see cref="ISsrfSafeConnector"/> for the connect-time re-validation that closes the
/// DNS-rebinding TOCTOU gap.
/// </summary>
public interface ISsrfGuard
{
    Task<SsrfCheckResult> ValidateHostAsync(string host, CancellationToken ct = default);
    bool IsDisallowedAddress(IPAddress address);
}
