namespace ProjectFlowAI.Domain.Entities;

// ---------------------------------------------------------------------------
// Phase 6: AI chat
// ---------------------------------------------------------------------------

public class AiChatConversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
    public User? User { get; set; }
    public ICollection<AiChatMessage> Messages { get; set; } = new List<AiChatMessage>();
}

public class AiChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public AiChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AiChatConversation? Conversation { get; set; }
}

// ---------------------------------------------------------------------------
// Phase 6: Workflow automation
// ---------------------------------------------------------------------------

public class WorkflowDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    /// <summary>Null => org-wide workflow, evaluated for every project's matching trigger.</summary>
    public Guid? ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkflowTriggerType TriggerType { get; set; }
    /// <summary>For TriggerType.Scheduled this holds the CronExpression; unused/empty for every other trigger type.</summary>
    public string TriggerConfigJson { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Organization? Organization { get; set; }
    public Project? Project { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<WorkflowCondition> Conditions { get; set; } = new List<WorkflowCondition>();
    public ICollection<WorkflowAction> Actions { get; set; } = new List<WorkflowAction>();
    public ICollection<WorkflowRun> Runs { get; set; } = new List<WorkflowRun>();
}

/// <summary>All conditions on a WorkflowDefinition are ANDed together — no nested groups.</summary>
public class WorkflowCondition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowDefinitionId { get; set; }
    /// <summary>e.g. "Priority", "Status", "AssigneeUserId", "StoryPoints", "Type".</summary>
    public string FieldPath { get; set; } = string.Empty;
    public WorkflowConditionOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;

    public WorkflowDefinition? WorkflowDefinition { get; set; }
}

public class WorkflowAction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowDefinitionId { get; set; }
    public WorkflowActionType ActionType { get; set; }
    /// <summary>Shape depends on ActionType — e.g. ChangeStatus: {"status":"Done"}; CallWebhook: {"url":"...","method":"POST"}; RequireApproval: {"approverUserId":"..."}.</summary>
    public string ActionConfigJson { get; set; } = string.Empty;
    /// <summary>Execution order within the workflow (ascending).</summary>
    public int Order { get; set; }

    public WorkflowDefinition? WorkflowDefinition { get; set; }
}

public class WorkflowRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowDefinitionId { get; set; }
    /// <summary>The WorkItem that triggered this run; null for Scheduled runs (no single triggering entity).</summary>
    public Guid? TriggerEntityId { get; set; }
    public WorkflowRunStatus Status { get; set; } = WorkflowRunStatus.Running;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    /// <summary>JSON array of {action, result, timestamp} entries, appended as each action executes.</summary>
    public string LogJson { get; set; } = "[]";

    public WorkflowDefinition? WorkflowDefinition { get; set; }
    public ICollection<WorkflowApprovalRequest> ApprovalRequests { get; set; } = new List<WorkflowApprovalRequest>();
}

public class WorkflowApprovalRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowRunId { get; set; }
    public Guid RequestedApproverUserId { get; set; }
    public WorkflowApprovalStatus Status { get; set; } = WorkflowApprovalStatus.Pending;
    public DateTime? DecidedAt { get; set; }
    public Guid? DecidedByUserId { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public WorkflowRun? WorkflowRun { get; set; }
}
