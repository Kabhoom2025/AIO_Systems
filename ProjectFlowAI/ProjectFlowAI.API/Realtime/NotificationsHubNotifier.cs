using Microsoft.AspNetCore.SignalR;
using ProjectFlowAI.API.Hubs;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Realtime;

/// <summary>Implements INotificationRealtimeNotifier using IHubContext&lt;NotificationsHub&gt;.</summary>
public class NotificationsHubNotifier : INotificationRealtimeNotifier
{
    private readonly IHubContext<NotificationsHub> _hub;

    public NotificationsHubNotifier(IHubContext<NotificationsHub> hub) => _hub = hub;

    public Task NotificationReceivedAsync(Guid userId, object notification, CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(NotificationsHub.UserGroup(userId)).SendAsync("NotificationReceived", notification, cancellationToken);
}
