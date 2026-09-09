using Microsoft.AspNetCore.SignalR;

namespace LinkShield.API.Hubs;

/// <summary>
/// Pushes scan-stage updates (QUEUED -> ... -> COMPLETED, see spec section 20) to clients
/// watching a specific scan. Callers join the "scan:{scanId}" group after issuing the scan.
/// </summary>
public class ScanProgressHub : Hub
{
    public async Task JoinScanGroup(string scanId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(scanId));
    }

    public async Task LeaveScanGroup(string scanId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(scanId));
    }

    public static string GroupName(string scanId) => $"scan:{scanId}";
}
