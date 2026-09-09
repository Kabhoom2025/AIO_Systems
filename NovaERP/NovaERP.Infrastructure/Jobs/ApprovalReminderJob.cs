using Microsoft.EntityFrameworkCore;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Jobs;

/// <summary>Recurring job (registered with Hangfire via RecurringJob.AddOrUpdate&lt;ApprovalReminderJob&gt;)
/// that nudges approvers on WorkflowStepInstance rows that have sat Pending for over 24 hours.
/// Plain class resolved per-run from DI (registered AddScoped) — this is the standard Hangfire
/// pattern for jobs that need scoped services like a DbContext.</summary>
public class ApprovalReminderJob
{
    public const string JobKey = "ApprovalReminder";

    private readonly NovaErpDbContext _ctx;
    private readonly INotificationService _notificationService;

    public ApprovalReminderJob(NovaErpDbContext ctx, INotificationService notificationService)
    {
        _ctx = ctx;
        _notificationService = notificationService;
    }

    public async Task RunAsync()
    {
        var status = "Success";
        try
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);

            var staleSteps = await _ctx.WorkflowStepInstances
                .Where(s => s.Status == "Pending" && s.WorkflowInstance.SubmittedDate < cutoff)
                .Select(s => new { Step = s, s.WorkflowInstance })
                .ToListAsync();

            if (staleSteps.Count > 0)
            {
                var definitionIds = staleSteps.Select(x => x.WorkflowInstance.WorkflowDefinitionId).Distinct().ToList();
                var orgIdsByDefinitionId = await _ctx.WorkflowDefinitions
                    .Where(d => definitionIds.Contains(d.Id))
                    .ToDictionaryAsync(d => d.Id, d => new { d.OrganizationId, d.EntityType });

                foreach (var entry in staleSteps)
                {
                    if (entry.Step.ApproverRoleId == null) continue;
                    if (!orgIdsByDefinitionId.TryGetValue(entry.WorkflowInstance.WorkflowDefinitionId, out var defInfo)) continue;

                    var approvers = await _ctx.Users
                        .Where(u => u.OrganizationId == defInfo.OrganizationId && u.RoleId == entry.Step.ApproverRoleId && u.IsActive)
                        .ToListAsync();

                    foreach (var approver in approvers)
                    {
                        await _notificationService.CreateAsync(defInfo.OrganizationId, new CreateNotificationDto
                        {
                            UserId = approver.Id,
                            Title = "Approval reminder",
                            Message = $"Workflow '{defInfo.EntityType}' #{entry.WorkflowInstance.EntityId} has been pending your approval for over 24 hours.",
                            Type = "Warning"
                        });
                    }
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
