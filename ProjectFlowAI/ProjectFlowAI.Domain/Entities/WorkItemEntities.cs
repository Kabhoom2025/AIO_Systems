using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Domain.Entities;

/// <summary>Named WorkItem (not "Task") to avoid collisions with System.Threading.Tasks.Task in handler code.</summary>
public class WorkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid? ParentWorkItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Backlog;
    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;
    public WorkItemType Type { get; set; } = WorkItemType.Task;
    public int? StoryPoints { get; set; }
    public decimal? EstimatedHours { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public Guid ReporterUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsRecurring { get; set; }
    public int? RecurrenceIntervalDays { get; set; }
    /// <summary>Sparse fractional position within its Status column for stable Kanban drag-drop ordering.</summary>
    public double Position { get; set; }
    /// <summary>Null => Backlog item not yet assigned to any Sprint (Phase 3 Scrum board).</summary>
    public Guid? SprintId { get; set; }
    /// <summary>Gantt chart start; DueDate doubles as the Gantt end. Null is a valid "not yet scheduled" state — callers fall back to CreatedAt.</summary>
    public DateTime? StartDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Project? Project { get; set; }
    public Sprint? Sprint { get; set; }
    public WorkItem? ParentWorkItem { get; set; }
    public User? AssigneeUser { get; set; }
    public User? ReporterUser { get; set; }
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();
    public ICollection<WorkItemLabel> WorkItemLabels { get; set; } = new List<WorkItemLabel>();
    public ICollection<WorkItemFollower> Followers { get; set; } = new List<WorkItemFollower>();
    public ICollection<WorkItemDependency> Dependencies { get; set; } = new List<WorkItemDependency>();
    public ICollection<WorkItemComment> Comments { get; set; } = new List<WorkItemComment>();
    public ICollection<WorkItemAttachment> Attachments { get; set; } = new List<WorkItemAttachment>();
    public ICollection<WorkItemActivity> Activities { get; set; } = new List<WorkItemActivity>();
    public ICollection<WorkItemTimeLog> TimeLogs { get; set; } = new List<WorkItemTimeLog>();
    public ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();
}

public class ChecklistItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public int Position { get; set; }

    public WorkItem? WorkItem { get; set; }
}

public class WorkItemLabel
{
    public Guid WorkItemId { get; set; }
    public Guid LabelId { get; set; }

    public WorkItem? WorkItem { get; set; }
    public Label? Label { get; set; }
}

public class WorkItemFollower
{
    public Guid WorkItemId { get; set; }
    public Guid UserId { get; set; }

    public WorkItem? WorkItem { get; set; }
    public User? User { get; set; }
}

/// <summary>Self-referencing join: WorkItemId depends on DependsOnWorkItemId. Must never have WorkItemId == DependsOnWorkItemId.</summary>
public class WorkItemDependency
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public Guid DependsOnWorkItemId { get; set; }
    public WorkItemDependencyType DependencyType { get; set; } = WorkItemDependencyType.Blocks;

    public WorkItem? WorkItem { get; set; }
    public WorkItem? DependsOnWorkItem { get; set; }
}

public class WorkItemComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public WorkItem? WorkItem { get; set; }
    public User? AuthorUser { get; set; }
    public ICollection<WorkItemCommentMention> Mentions { get; set; } = new List<WorkItemCommentMention>();
}

/// <summary>Explicit mentions resolved client-side (from the frontend's own @mention autocomplete) — no server-side text parsing.</summary>
public class WorkItemCommentMention
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CommentId { get; set; }
    public Guid MentionedUserId { get; set; }

    public WorkItemComment? Comment { get; set; }
    public User? MentionedUser { get; set; }
}

public class WorkItemAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public WorkItem? WorkItem { get; set; }
    public User? UploadedByUser { get; set; }
}

/// <summary>Auto-recorded by every mutating WorkItem command handler so the frontend can render a real activity timeline.</summary>
public class WorkItemActivity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public WorkItem? WorkItem { get; set; }
    public User? User { get; set; }
}

/// <summary>Manual time entry. "Actual hours" for a WorkItem = SUM(Minutes)/60 across its TimeLogs, computed on read — never denormalized.</summary>
public class WorkItemTimeLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public Guid UserId { get; set; }
    public int Minutes { get; set; }
    public string? Note { get; set; }
    public DateOnly LoggedDate { get; set; }
    public bool IsBillable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public WorkItem? WorkItem { get; set; }
    public User? User { get; set; }
}

// --- Custom fields ---

public class CustomFieldDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; } = CustomFieldType.Text;
    /// <summary>JSON array of strings; only used when FieldType == Dropdown.</summary>
    public string? OptionsJson { get; set; }
    public bool IsRequired { get; set; }

    public Project? Project { get; set; }
    public ICollection<CustomFieldValue> Values { get; set; } = new List<CustomFieldValue>();
}

public class CustomFieldValue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public Guid CustomFieldDefinitionId { get; set; }
    public string ValueJson { get; set; } = string.Empty;

    public WorkItem? WorkItem { get; set; }
    public CustomFieldDefinition? CustomFieldDefinition { get; set; }
}
