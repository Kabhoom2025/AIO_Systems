using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LinkShield.Worker;

/// <summary>
/// Runs independently of LinkShield.API's in-process scan queue — its job is to catch what
/// that queue can't: scans left mid-pipeline because the API process restarted (the in-memory
/// Channel queue doesn't survive a restart), and old soft-deleted rows that should eventually
/// be purged. Spec section 17: "cleanup" and "scheduled reputation updates" background workers.
/// </summary>
public class MaintenanceWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MaintenanceWorker> _logger;
    private readonly TimeSpan _interval;
    private readonly TimeSpan _stuckScanThreshold;
    private readonly TimeSpan _softDeleteRetention;

    public MaintenanceWorker(IServiceScopeFactory scopeFactory, ILogger<MaintenanceWorker> logger, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromMinutes(configuration.GetValue("Maintenance:IntervalMinutes", 15));
        _stuckScanThreshold = TimeSpan.FromMinutes(configuration.GetValue("Maintenance:StuckScanThresholdMinutes", 10));
        _softDeleteRetention = TimeSpan.FromDays(configuration.GetValue("Maintenance:SoftDeleteRetentionDays", 30));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        do
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Maintenance pass failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkShieldDbContext>();

        var stuckBefore = DateTime.UtcNow - _stuckScanThreshold;
        var stuckScans = await db.UrlScans
            .Where(s => s.Status != ScanStatus.Completed && s.Status != ScanStatus.Failed && s.CreatedAtUtc < stuckBefore)
            .ToListAsync(ct);

        foreach (var scan in stuckScans)
        {
            scan.Status = ScanStatus.Failed;
            scan.FailureReason = "Scan timed out without completing (worker cleanup).";
            db.ScanEvents.Add(new Domain.Entities.ScanEvent
            {
                UrlScanId = scan.Id,
                Stage = ScanStatus.Failed,
                Message = "Marked failed by MaintenanceWorker — exceeded stuck-scan threshold."
            });
        }

        if (stuckScans.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Marked {Count} stuck scan(s) as Failed.", stuckScans.Count);
        }

        var purgeBefore = DateTime.UtcNow - _softDeleteRetention;
        var purgeable = await db.UrlScans
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted && s.DeletedAtUtc != null && s.DeletedAtUtc < purgeBefore)
            .ToListAsync(ct);

        if (purgeable.Count > 0)
        {
            db.UrlScans.RemoveRange(purgeable);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Purged {Count} soft-deleted scan(s) older than {Days} day(s).", purgeable.Count, _softDeleteRetention.TotalDays);
        }
    }
}
