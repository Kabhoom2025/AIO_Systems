namespace FlowSphere.Application.Workflows.Queries.GetWorkflowsList;

public record WorkflowSummaryDto(int Id, string Name, string? Description, bool IsEnabled, DateTime CreatedDate);
