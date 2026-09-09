using Microsoft.AspNetCore.SignalR;
using Pharmacy.API.Hubs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.BackgroundServices;

public class NotificationSweepService : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<NotificationsHub> _hub;
    private readonly ILogger<NotificationSweepService> _logger;

    public NotificationSweepService(
        IServiceScopeFactory scopeFactory,
        IHubContext<NotificationsHub> hub,
        ILogger<NotificationSweepService> logger)
    {
        _scopeFactory = scopeFactory;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SweepInterval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification sweep failed");
            }
        }
    }

    private async Task SweepAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var orgRepo = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var orgIds = await orgRepo.GetAllActiveIdsAsync();
        foreach (var orgId in orgIds)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var newOnes = await notificationService.GenerateAndGetNewAsync(orgId);
            if (newOnes.Count == 0) continue;

            var unreadCount = await notificationService.GetUnreadCountAsync(orgId);
            await _hub.Clients.Group($"org-{orgId}").SendAsync("notificationsUpdated", new
            {
                unreadCount,
                notifications = newOnes
            }, stoppingToken);
        }
    }
}
