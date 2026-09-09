using System.Net.Sockets;
using LinkShield.Application.Interfaces;

namespace LinkShield.Infrastructure.Security;

public class SsrfSafeConnector : ISsrfSafeConnector
{
    private readonly ISsrfGuard _guard;

    public SsrfSafeConnector(ISsrfGuard guard)
    {
        _guard = guard;
    }

    public async Task<Socket> ConnectAsync(string host, int port, CancellationToken ct = default)
    {
        var check = await _guard.ValidateHostAsync(host, ct);
        if (!check.IsAllowed)
            throw new SsrfBlockedException(check.Reason ?? $"Host '{host}' rejected.");

        Exception? lastError = null;
        foreach (var address in check.ResolvedAddresses)
        {
            // Re-check each address at connect time — the ISsrfGuard result may be a few
            // milliseconds old (DNS-rebinding TOCTOU); never trust it without re-verifying here.
            if (_guard.IsDisallowedAddress(address)) continue;

            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(address, port, ct);
                return socket;
            }
            catch (Exception ex)
            {
                lastError = ex;
                socket.Dispose();
            }
        }

        throw new SsrfBlockedException(
            lastError is null
                ? $"No allowed address for '{host}' could be connected to."
                : $"Could not connect to any allowed address for '{host}': {lastError.Message}");
    }
}
