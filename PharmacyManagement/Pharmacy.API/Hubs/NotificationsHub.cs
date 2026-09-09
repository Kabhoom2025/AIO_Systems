using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Pharmacy.API.Hubs;

[Authorize]
public class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var orgId = Context.User?.FindFirst("organizationId")?.Value;
        if (!string.IsNullOrEmpty(orgId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"org-{orgId}");

        await base.OnConnectedAsync();
    }
}
