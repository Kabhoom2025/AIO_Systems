using Microsoft.EntityFrameworkCore;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.API.BackgroundServices;

/// <summary>Periodically raises in-app notifications for things that need attention.
/// Foundation phase: sweeps for user accounts still locked out past their lockout window
/// and clears stale locks, notifying org admins so nothing needs a DB console to unblock.</summary>
public class NotificationSweepService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationSweepService> _logger;

    public NotificationSweepService(IServiceScopeFactory scopeFactory, ILogger<NotificationSweepService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunSweepAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification sweep failed");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // shutting down
            }
        }
    }

    private async Task RunSweepAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();

        var now = DateTime.UtcNow;

        var expiredLocks = await ctx.Users
            .Where(u => u.LockedUntil != null && u.LockedUntil <= now)
            .ToListAsync(ct);

        foreach (var user in expiredLocks)
        {
            user.LockedUntil = null;
            user.FailedLoginCount = 0;

            var alreadyNotified = await ctx.Notifications.AnyAsync(n =>
                n.OrganizationId == user.OrganizationId &&
                n.Title == "Account lockout cleared" &&
                n.Message.Contains(user.Email), ct);

            if (!alreadyNotified)
            {
                ctx.Notifications.Add(new Notification
                {
                    OrganizationId = user.OrganizationId,
                    UserId         = null, // broadcast to org admins
                    Title          = "Account lockout cleared",
                    Message        = $"{user.Email}'s account lockout has expired and is now unlocked.",
                    Type           = "Info"
                });
            }
        }

        if (ctx.ChangeTracker.HasChanges())
            await ctx.SaveChangesAsync(ct);
    }
}
