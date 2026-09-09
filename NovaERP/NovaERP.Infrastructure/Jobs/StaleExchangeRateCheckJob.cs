using Microsoft.EntityFrameworkCore;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Jobs;

/// <summary>Recurring job (registered with Hangfire via RecurringJob.AddOrUpdate&lt;StaleExchangeRateCheckJob&gt;)
/// that warns each org's Admins when any of their ExchangeRate rows are older than 30 days.</summary>
public class StaleExchangeRateCheckJob
{
    public const string JobKey = "StaleExchangeRateCheck";

    private readonly NovaErpDbContext _ctx;
    private readonly INotificationService _notificationService;

    public StaleExchangeRateCheckJob(NovaErpDbContext ctx, INotificationService notificationService)
    {
        _ctx = ctx;
        _notificationService = notificationService;
    }

    public async Task RunAsync()
    {
        var status = "Success";
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var orgs = await _ctx.Organizations.ToListAsync();

            foreach (var org in orgs)
            {
                var hasStaleRates = await _ctx.ExchangeRates
                    .AnyAsync(r => r.OrganizationId == org.Id && r.EffectiveDate < cutoff);
                if (!hasStaleRates) continue;

                // Seed data names the system Admin role "Admin" — see SeedData.cs adminRole.
                var adminUsers = await _ctx.Users
                    .Where(u => u.OrganizationId == org.Id && u.IsActive && u.Role.Name == "Admin")
                    .ToListAsync();

                foreach (var admin in adminUsers)
                {
                    await _notificationService.CreateAsync(org.Id, new CreateNotificationDto
                    {
                        UserId = admin.Id,
                        Title = "Exchange rates need refreshing",
                        Message = "One or more exchange rates for your organization are older than 30 days and should be reviewed.",
                        Type = "Warning"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            status = $"Failed: {ex.Message}";
        }

        await UpdateJobStatusAsync(status);
    }

    private async Task UpdateJobStatusAsync(string status)
    {
        var jobDef = await _ctx.ScheduledJobDefinitions.FirstOrDefaultAsync(j => j.JobKey == JobKey);
        if (jobDef == null) return;
        jobDef.LastRunAt = DateTime.UtcNow;
        jobDef.LastRunStatus = status;
        await _ctx.SaveChangesAsync();
    }
}
