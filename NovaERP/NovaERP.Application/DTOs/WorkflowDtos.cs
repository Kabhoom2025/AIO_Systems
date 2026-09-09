namespace NovaERP.Application.DTOs;

public class WorkflowStepDefinitionDto
{
    public int      Id             { get; set; }
    public int      StepOrder      { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public int?     ApproverRoleId { get; set; }
    public string?  ApproverRoleName { get; set; }
    public decimal? MinAmount      { get; set; }
}

public class CreateWorkflowStepDefinitionDto
{
    public int      StepOrder      { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public int?     ApproverRoleId { get; set; }
    public decimal? MinAmount      { get; set; }
}

public class WorkflowDefinitionDto
{
    public int    Id         { get; set; }
    public string Name       { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool   IsActive   { get; set; }
    public List<WorkflowStepDefinitionDto> Steps { get; set; } = new();
}

public class CreateWorkflowDefinitionDto
{
    public string Name       { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool   IsActive   { get; set; } = true;
    public List<CreateWorkflowStepDefinitionDto> Steps { get; set; } = new();
}

/// <summary>Steps are managed as part of the definition and replaced wholesale on update —
/// same "replace-all" approach as UpdateOrganizationLanguagesDto, simpler than a separate
/// step CRUD API for what is a small, definition-owned collection.</summary>
public class UpdateWorkflowDefinitionDto
{
    public string Name       { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool   IsActive   { get; set; } = true;
    public List<CreateWorkflowStepDefinitionDto> Steps { get; set; } = new();
}

public class WorkflowStepInstanceDto
{
    public int       Id               { get; set; }
    public int       StepOrder        { get; set; }
    public int?      ApproverRoleId   { get; set; }
    public string?   ApproverRoleName { get; set; }
    public int?      ApproverUserId   { get; set; }
    public string?   ApproverUserName { get; set; }
    public string    Status           { get; set; } = string.Empty;
    public DateTime? ActionedDate     { get; set; }
    public string?   Comments         { get; set; }
}

public class WorkflowInstanceDto
{
    public int      Id                   { get; set; }
    public int      WorkflowDefinitionId { get; set; }
    public string   WorkflowDefinitionName { get; set; } = string.Empty;
    public string   EntityType           { get; set; } = string.Empty;
    public int      EntityId             { get; set; }
    public decimal? Amount               { get; set; }
    public string   Status               { get; set; } = string.Empty;
    public int      CurrentStepOrder     { get; set; }
    public int      SubmittedByUserId    { get; set; }
    public DateTime SubmittedDate        { get; set; }
    public DateTime? CompletedDate       { get; set; }
    public List<WorkflowStepInstanceDto> Steps { get; set; } = new();
}

public class StartWorkflowDto
{
    public string   EntityType { get; set; } = string.Empty;
    public int      EntityId   { get; set; }
    public decimal? Amount     { get; set; }
}

/// <summary>Shared by both the approve and reject endpoints — Comments is optional in either case.</summary>
public class WorkflowActionDto
{
    public string? Comments { get; set; }
}
