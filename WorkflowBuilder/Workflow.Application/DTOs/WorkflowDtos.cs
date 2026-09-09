namespace Workflow.Application.DTOs;

public class WorkflowDefinitionDto
{
    public int      Id                    { get; set; }
    public string   Name                  { get; set; } = string.Empty;
    public string?  Description           { get; set; }
    public string   TriggerType           { get; set; } = string.Empty;
    public string   TriggerLabel          { get; set; } = string.Empty;
    public bool     IsActive              { get; set; }
    public int?     PublishedVersionId    { get; set; }
    public int?     PublishedVersionNumber { get; set; }
    public int      DraftVersionId        { get; set; }
    public int      DraftVersionNumber    { get; set; }
    public DateTime? PublishedAt          { get; set; }
    public DateTime UpdatedDate           { get; set; }
    public string   WebhookToken          { get; set; } = string.Empty;
}

public class CreateWorkflowDefinitionDto
{
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string  TriggerType { get; set; } = string.Empty;
}

public class UpdateWorkflowDefinitionDto
{
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool    IsActive    { get; set; } = true;
}

public class WorkflowVersionDto
{
    public int       Id              { get; set; }
    public int       VersionNumber   { get; set; }
    public string    GraphJson       { get; set; } = string.Empty;
    public string    Status          { get; set; } = string.Empty;
    public DateTime? PublishedAt     { get; set; }
    public string?   PublishedByName { get; set; }
    public DateTime  CreatedDate     { get; set; }
}

public class SaveDraftGraphDto
{
    public string GraphJson { get; set; } = "{\"nodes\":[],\"edges\":[]}";
}

public class WorkflowFieldDto
{
    public string  Key     { get; set; } = string.Empty;
    public string  Label   { get; set; } = string.Empty;
    /// <summary>string | number | boolean</summary>
    public string  Type    { get; set; } = "string";
    public List<string>? Options { get; set; }
}

public class TriggerTypeDto
{
    public string Key   { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<WorkflowFieldDto> Fields { get; set; } = new();
}

public class ActionTypeDto
{
    public string Key           { get; set; } = string.Empty;
    public string Label         { get; set; } = string.Empty;
    public List<WorkflowFieldDto> ConfigFields { get; set; } = new();
}

public class WorkflowExecutionDto
{
    public int      Id                { get; set; }
    public string   TriggerEntityType { get; set; } = string.Empty;
    public int      TriggerEntityId   { get; set; }
    public string   Status            { get; set; } = string.Empty;
    public string?  ErrorMessage      { get; set; }
    public string   PathJson          { get; set; } = string.Empty;
    public DateTime CreatedDate       { get; set; }
}

/// <summary>Arbitrary field/value context supplied by the caller when manually test-running a
/// workflow, or the parsed body of an inbound webhook call.</summary>
public class RunWorkflowDto
{
    public Dictionary<string, object?> Context { get; set; } = new();
}
