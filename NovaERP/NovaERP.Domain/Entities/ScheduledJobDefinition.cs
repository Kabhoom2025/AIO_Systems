namespace NovaERP.Domain.Entities;

/// <summary>A Hangfire recurring job registration, persisted so its schedule survives restarts
/// and is admin-editable. OrganizationId null means the job is platform-wide (applies across all
/// orgs internally, e.g. by looping every org inside the job body) rather than scoped to one org.</summary>
public class ScheduledJobDefinition : BaseEntity
{
    public int?    OrganizationId  { get; set; }
    public string  Name            { get; set; } = string.Empty;
    public string  JobKey          { get; set; } = string.Empty; // e.g. "ApprovalReminder", "StaleExchangeRateCheck"
    public string  CronExpression  { get; set; } = string.Empty; // standard 5-field cron
    public bool    IsEnabled       { get; set; } = true;
    public DateTime? LastRunAt     { get; set; }
    public string?   LastRunStatus { get; set; } // "Success" | "Failed" | error message

    public Organization? Organization { get; set; }
}
