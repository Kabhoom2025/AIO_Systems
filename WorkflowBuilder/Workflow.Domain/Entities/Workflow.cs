namespace Workflow.Domain.Entities;

/// <summary>A named, versioned automation (e.g. "Order Follow-up"). The actual node/edge graph
/// lives on <see cref="WorkflowVersion"/> — this row is just the stable identity + which version
/// is currently live.</summary>
public class WorkflowDefinition : BaseEntity
{
    public int     OrganizationId      { get; set; } = 1;
    public string  Name                { get; set; } = string.Empty;
    public string? Description         { get; set; }
    /// <summary>Key into the trigger catalog, e.g. "Manual" or "Webhook".</summary>
    public string  TriggerType         { get; set; } = string.Empty;
    public bool    IsActive            { get; set; } = true;
    public int?    PublishedVersionId  { get; set; }
    /// <summary>Opaque token used in the public webhook URL for this workflow.</summary>
    public string  WebhookToken        { get; set; } = Guid.NewGuid().ToString("N");

    public WorkflowVersion? PublishedVersion { get; set; }
    public ICollection<WorkflowVersion> Versions { get; set; } = new List<WorkflowVersion>();
}

/// <summary>One snapshot of the node/edge graph. Editing always happens on the Draft version;
/// Publish freezes it (Status → Published) and starts a fresh Draft copy for further edits.</summary>
public class WorkflowVersion : BaseEntity
{
    public int     WorkflowDefinitionId { get; set; }
    public int     VersionNumber        { get; set; }
    /// <summary>Serialized { nodes: [...], edges: [...] } graph — see WorkflowEngine for the shape.</summary>
    public string  GraphJson            { get; set; } = "{\"nodes\":[],\"edges\":[]}";
    public string  Status               { get; set; } = "Draft"; // Draft | Published | Archived
    public DateTime? PublishedAt        { get; set; }
    public int?    PublishedByUserId    { get; set; }

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public User? PublishedByUser { get; set; }
}

/// <summary>One run of a published (or, for test-runs, draft) workflow — powers an audit trail of
/// which path a run took and what it did.</summary>
public class WorkflowExecution : BaseEntity
{
    public int     OrganizationId      { get; set; } = 1;
    public int     WorkflowDefinitionId { get; set; }
    public int     WorkflowVersionId   { get; set; }
    /// <summary>"Manual" | "Webhook" — how this run was kicked off.</summary>
    public string  TriggerEntityType   { get; set; } = string.Empty;
    public int     TriggerEntityId     { get; set; }
    public string  Status              { get; set; } = "Completed"; // Completed | Failed
    public string? ErrorMessage        { get; set; }
    /// <summary>Ordered JSON array of steps taken: node ids visited, branch decisions, actions executed.</summary>
    public string  PathJson            { get; set; } = "[]";

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public WorkflowVersion    WorkflowVersion    { get; set; } = null!;
}
