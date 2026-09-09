using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Domain.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid OwnerUserId { get; set; }
    public bool IsArchived { get; set; }
    /// <summary>Optional default billing rate for this project's time logs (Phase 3 time tracking) — informational only, not currently multiplied into any total by the API.</summary>
    public decimal? DefaultHourlyRate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Organization? Organization { get; set; }
    public User? OwnerUser { get; set; }
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public ICollection<Label> Labels { get; set; } = new List<Label>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    public ICollection<CustomFieldDefinition> CustomFieldDefinitions { get; set; } = new List<CustomFieldDefinition>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<GanttBaseline> GanttBaselines { get; set; } = new List<GanttBaseline>();
}

public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string RoleInProject { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
    public User? User { get; set; }
}

public class Milestone
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
}

public class Label
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#6366F1";

    public Project? Project { get; set; }
    public ICollection<WorkItemLabel> WorkItemLabels { get; set; } = new List<WorkItemLabel>();
}
