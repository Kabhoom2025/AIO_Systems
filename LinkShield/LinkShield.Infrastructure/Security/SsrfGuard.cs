using System.Net;
using System.Net.Sockets;
using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LinkShield.Infrastructure.Security;

public class SsrfGuard : ISsrfGuard
{
    private readonly int _timeoutMs;

    public SsrfGuard(IConfiguration configuration)
    {
        _timeoutMs = int.TryParse(configuration["SsrfProtection:RequestTimeoutMs"], out var t) ? t : 5000;
    }

    public async Task<SsrfCheckResult> ValidateHostAsync(string host, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            return SsrfCheckResult.Denied("Empty host.");

        if (LooksInternal(host))
            return SsrfCheckResult.Denied($"Host '{host}' looks like an internal/reserved name.");

        // A literal IP in the URL still has to pass the same address checks as a resolved name.
        if (IPAddress.TryParse(host, out var literalIp))
        {
            return IsDisallowedAddress(literalIp)
                ? SsrfCheckResult.Denied($"IP address {literalIp} is private/reserved.")
                : SsrfCheckResult.Allowed([literalIp]);
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeoutMs);

        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(host, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return SsrfCheckResult.Denied($"DNS resolution for '{host}' timed out.");
        }
        catch (SocketException ex)
        {
            return SsrfCheckResult.Denied($"DNS resolution for '{host}' failed: {ex.Message}");
        }

        if (addresses.Length == 0)
            return SsrfCheckResult.Denied($"Host '{host}' did not resolve to any address.");

        var disallowed = addresses.FirstOrDefault(IsDisallowedAddress);
        return disallowed is not null
            ? SsrfCheckResult.Denied($"Host '{host}' resolves to private/reserved address {disallowed}.")
            : SsrfCheckResult.Allowed(addresses);
    }

    public bool IsDisallowedAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address)) return true;
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            if (bytes[0] == 10) return true;                                  // 10.0.0.0/8
            if (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) return true;   // 172.16.0.0/12
            if (bytes[0] == 192 && bytes[1] == 168) return true;              // 192.168.0.0/16
            if (bytes[0] == 169 && bytes[1] == 254) return true;              // 169.254.0.0/16 (link-local + cloud metadata)
            if (bytes[0] == 127) return true;                                 // 127.0.0.0/8
            if (bytes[0] >= 224) return true;                                 // multicast (224+) / reserved (240+)
            if (bytes[0] == 0) return true;                                   // 0.0.0.0/8
            return false;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.IsIPv6LinkLocal) return true;      // fe80::/10
            if (address.IsIPv6Multicast) return true;
            if (address.IsIPv6SiteLocal) return true;
            var bytes = address.GetAddressBytes();
            if ((bytes[0] & 0xFE) == 0xFC) return true;     // fc00::/7 unique local
            return false;
        }

        return true; // unknown address family — fail closed
    }

    private static bool LooksInternal(string host)
    {
        if (!host.Contains('.')) return true; // single-label hostname, e.g. "localhost", "router"
        var lower = host.ToLowerInvariant();
        return lower.EndsWith(".local") || lower.EndsWith(".internal") || lower.EndsWith(".localhost") ||
               lower == "metadata.google.internal";
    }
}
