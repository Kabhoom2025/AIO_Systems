using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.BackgroundServices;

/// <summary>Periodically raises in-app notifications for things that need attention (expiring documents, pending approvals).</summary>
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
        var ctx = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();

        var soon = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expiringDocs = await ctx.EmployeeDocuments
            .Include(d => d.Employee)
            .Where(d => d.ExpiryDate != null && d.ExpiryDate >= today && d.ExpiryDate <= soon)
            .ToListAsync(ct);

        foreach (var doc in expiringDocs)
        {
            var alreadyNotified = await ctx.Notifications.AnyAsync(n =>
                n.OrganizationId == doc.Employee.OrganizationId &&
                n.Link == $"/employees/{doc.EmployeeId}" &&
                n.Title == "Document expiring soon" &&
                n.Message.Contains(doc.Name), ct);

            if (alreadyNotified) continue;

            ctx.Notifications.Add(new Notification
            {
                OrganizationId = doc.Employee.OrganizationId,
                UserId         = null, // broadcast to org (HR/admin dashboards)
                Title          = "Document expiring soon",
                Message        = $"{doc.Employee.FullName}'s {doc.Name} expires on {doc.ExpiryDate:yyyy-MM-dd}.",
                Type           = "Warning",
                Link           = $"/employees/{doc.EmployeeId}"
            });
        }

        if (ctx.ChangeTracker.HasChanges())
            await ctx.SaveChangesAsync(ct);
    }
}
