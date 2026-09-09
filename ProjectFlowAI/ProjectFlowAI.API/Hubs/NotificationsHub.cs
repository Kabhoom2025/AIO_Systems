using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ProjectFlowAI.API.Hubs;

/// <summary>Realtime notifications hub, mapped at /hubs/notifications. On connect, joins a group
/// keyed by the caller's own user id — NotificationDispatcher (Application layer) pushes
/// "NotificationReceived" events to that same group via INotificationRealtimeNotifier
/// (NotificationsHubNotifier) after it commits a new Notification row.</summary>
[Authorize]
public class NotificationsHub : Hub
{
    public static string UserGroup(Guid userId) => $"notif-user:{userId}";

    public override async Task OnConnectedAsync()
    {
        var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(value, out var userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

        await base.OnConnectedAsync();
    }
}
