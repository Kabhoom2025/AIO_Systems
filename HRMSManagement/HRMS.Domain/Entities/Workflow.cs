namespace HRMS.Domain.Entities;

/// <summary>A named, versioned automation attached to a business event (e.g. "New Leave Request
/// Submitted"). The actual node/edge graph lives on <see cref="WorkflowVersion"/> — this row is
/// just the stable identity + which version is currently live.</summary>
public class WorkflowDefinition : BaseEntity
{
    public int     OrganizationId      { get; set; }
    public string  Name                { get; set; } = string.Empty;
    public string? Description         { get; set; }
    /// <summary>Key into the trigger catalog, e.g. "LeaveRequestSubmitted".</summary>
    public string  TriggerType         { get; set; } = string.Empty;
    public bool    IsActive            { get; set; } = true;
    public int?    PublishedVersionId  { get; set; }

    public Organization Organization { get; set; } = null!;
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

/// <summary>One run of a published workflow against a real business record (e.g. a specific
/// LeaveRequest) — powers an audit trail of which path a run took and what it did.</summary>
public class WorkflowExecution : BaseEntity
{
    public int     OrganizationId      { get; set; }
    public int     WorkflowDefinitionId { get; set; }
    public int     WorkflowVersionId   { get; set; }
    public string  TriggerEntityType   { get; set; } = string.Empty; // e.g. "LeaveRequest"
    public int     TriggerEntityId     { get; set; }
    public string  Status              { get; set; } = "Completed"; // Completed | Failed
    public string? ErrorMessage        { get; set; }
    /// <summary>Ordered JSON array of steps taken: node ids visited, branch decisions, actions executed.</summary>
    public string  PathJson            { get; set; } = "[]";

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public WorkflowVersion    WorkflowVersion    { get; set; } = null!;
}
