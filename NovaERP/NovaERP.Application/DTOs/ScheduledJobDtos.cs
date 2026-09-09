namespace NovaERP.Application.DTOs;

public class ScheduledJobDefinitionDto
{
    public int       Id             { get; set; }
    public int?      OrganizationId { get; set; }
    public string    Name           { get; set; } = string.Empty;
    public string    JobKey         { get; set; } = string.Empty;
    public string    CronExpression { get; set; } = string.Empty;
    public bool      IsEnabled      { get; set; }
    public DateTime? LastRunAt      { get; set; }
    public string?   LastRunStatus  { get; set; }
}

public class UpdateScheduledJobDto
{
    public string CronExpression { get; set; } = string.Empty;
    public bool   IsEnabled      { get; set; }
}
