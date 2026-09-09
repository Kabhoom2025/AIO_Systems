using LinkShield.API.Hubs;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace LinkShield.API.Notifications;

public class SignalRScanNotifier : IScanNotifier
{
    private readonly IHubContext<ScanProgressHub> _hubContext;

    public SignalRScanNotifier(IHubContext<ScanProgressHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyStageAsync(Guid scanId, ScanStatus stage, string message, CancellationToken ct = default) =>
        _hubContext.Clients.Group(ScanProgressHub.GroupName(scanId.ToString()))
            .SendAsync("scanStageChanged", new { scanId, stage = stage.ToString(), message }, ct);
}
