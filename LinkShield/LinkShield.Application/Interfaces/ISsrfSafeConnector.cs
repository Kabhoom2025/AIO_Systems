using System.Net.Sockets;

namespace LinkShield.Application.Interfaces;

/// <summary>
/// Resolves + validates a host via <see cref="ISsrfGuard"/> and connects a raw socket to the
/// first allowed address — used by anything that needs a real TCP connection to a
/// user-influenced host (SSL/TLS analysis, the HttpClient ConnectCallback backing redirect
/// following and RDAP lookups). Re-validating at actual-connect-time (not just after an earlier
/// DNS lookup) is what closes the DNS-rebinding TOCTOU gap: a hostname that resolved safely a
/// moment ago could resolve to a private IP by the time the connection is opened.
/// </summary>
public interface ISsrfSafeConnector
{
    Task<Socket> ConnectAsync(string host, int port, CancellationToken ct = default);
}

public class SsrfBlockedException(string reason) : Exception($"Blocked by SSRF protection: {reason}");
