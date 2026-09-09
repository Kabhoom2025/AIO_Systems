using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Domain.Entities;

/// <summary>A fixed-length iteration scoping a subset of a Project's WorkItems (WorkItem.SprintId).
/// Only one Sprint per Project may be Active at a time (enforced in StartSprintCommandHandler,
/// not at the DB level, since "active" is a business rule rather than a structural constraint).</summary>
public class Sprint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; } = SprintStatus.Planned;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    public ICollection<RetrospectiveNote> RetrospectiveNotes { get; set; } = new List<RetrospectiveNote>();
}

/// <summary>One free-text note captured during a Sprint's retrospective ceremony.</summary>
public class RetrospectiveNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SprintId { get; set; }
    public RetrospectiveCategory Category { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Sprint? Sprint { get; set; }
    public User? CreatedByUser { get; set; }
}

/// <summary>The one currently-running stopwatch-style timer for a user (unique index on UserId —
/// a user can only have a single running timer). Deleted (not soft-deleted) the moment it's stopped,
/// at which point a real WorkItemTimeLog row is created via the same path as manual time entry.</summary>
public class ActiveTimer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid WorkItemId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public WorkItem? WorkItem { get; set; }
}

/// <summary>A named snapshot of a Project's planned schedule (Gantt "save baseline"), so the
/// current live schedule can later be compared against what was originally planned.</summary>
public class GanttBaseline
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
    public ICollection<GanttBaselineItem> Items { get; set; } = new List<GanttBaselineItem>();
}

/// <summary>One WorkItem's planned start/end as of the moment its GanttBaseline was taken. Title is
/// deliberately denormalized (copied at snapshot time) since the live WorkItem's title may later
/// change and the baseline must keep describing what was actually planned back then.</summary>
public class GanttBaselineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BaselineId { get; set; }
    public Guid WorkItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }

    public GanttBaseline? Baseline { get; set; }
    public WorkItem? WorkItem { get; set; }
}
