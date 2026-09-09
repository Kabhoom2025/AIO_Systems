using Hangfire;

namespace NovaERP.Infrastructure.Jobs;

/// <summary>Single place mapping a ScheduledJobDefinition.JobKey string to the Hangfire recurring
/// job registration call for the matching job class. Used both on startup (Program.cs, for every
/// enabled ScheduledJobDefinition row) and from ScheduledJobController.Update (to re-register a
/// job immediately when its cron/enabled state changes), so the two call sites can never drift.</summary>
public static class RecurringJobRegistrar
{
    /// <summary>Deterministic Hangfire recurring-job id for one ScheduledJobDefinition row —
    /// combining JobKey + row Id supports multiple org-scoped rows sharing the same JobKey.</summary>
    public static string RecurringJobId(string jobKey, int id) => $"{jobKey}-{id}";

    public static void Register(string jobKey, int id, string cronExpression, bool isEnabled)
    {
        var recurringJobId = RecurringJobId(jobKey, id);

        if (!isEnabled)
        {
            RecurringJob.RemoveIfExists(recurringJobId);
            return;
        }

        switch (jobKey)
        {
            case ApprovalReminderJob.JobKey:
                RecurringJob.AddOrUpdate<ApprovalReminderJob>(recurringJobId, j => j.RunAsync(), cronExpression);
                break;
            case StaleExchangeRateCheckJob.JobKey:
                RecurringJob.AddOrUpdate<StaleExchangeRateCheckJob>(recurringJobId, j => j.RunAsync(), cronExpression);
                break;
            default:
                throw new InvalidOperationException($"Unknown JobKey '{jobKey}' — no job class is registered for it.");
        }
    }

    public static string TriggerNow(string jobKey) => jobKey switch
    {
        ApprovalReminderJob.JobKey => BackgroundJob.Enqueue<ApprovalReminderJob>(j => j.RunAsync()),
        StaleExchangeRateCheckJob.JobKey => BackgroundJob.Enqueue<StaleExchangeRateCheckJob>(j => j.RunAsync()),
        _ => throw new InvalidOperationException($"Unknown JobKey '{jobKey}' — no job class is registered for it.")
    };
}
