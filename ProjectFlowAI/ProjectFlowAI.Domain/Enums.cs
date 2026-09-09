namespace ProjectFlowAI.Domain;

public enum UserStatus
{
    Active,
    Invited,
    Suspended,
    Deactivated
}

public enum SubscriptionPlan
{
    Free,
    Starter,
    Business,
    Enterprise
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Revoked,
    Expired
}

public enum ProjectStatus
{
    Planning,
    Active,
    OnHold,
    Completed,
    Archived
}

public enum MilestoneStatus
{
    Open,
    Completed
}

/// <summary>Exactly the 7 Kanban columns this product's board spec defines — do not add/remove without updating the board UI.</summary>
public enum WorkItemStatus
{
    Backlog,
    ToDo,
    InProgress,
    CodeReview,
    Testing,
    Blocked,
    Done
}

public enum WorkItemPriority
{
    Lowest,
    Low,
    Medium,
    High,
    Highest
}

public enum WorkItemType
{
    Task,
    Bug,
    Story,
    Epic
}

public enum WorkItemDependencyType
{
    Blocks,
    RelatesTo
}

public enum CustomFieldType
{
    Text,
    Number,
    Date,
    Dropdown,
    Checkbox
}

// ---------------------------------------------------------------------------
// Phase 3: Scrum / Gantt / Calendar / Time Tracking
// ---------------------------------------------------------------------------

public enum SprintStatus
{
    Planned,
    Active,
    Completed
}

public enum RetrospectiveCategory
{
    WentWell,
    WentWrong,
    ActionItem
}

/// <summary>Drives the frontend calendar's event-type badge/color; string-serialized like every
/// other enum in this API (JsonStringEnumConverter is global).</summary>
public enum CalendarEventType
{
    WorkItemDue,
    Milestone,
    SprintStart,
    SprintEnd
}

// ---------------------------------------------------------------------------
// Phase 4: Chat / Notifications / Documents / Wiki
// ---------------------------------------------------------------------------

public enum NotificationType
{
    Mention,
    Comment,
    Assignment,
    Invitation,
    SprintStarted,
    DueDateApproaching,
    General,
    WorkflowAutomation
}

public enum NotificationChannel
{
    InApp,
    Email,
    Slack,
    Teams,
    Sms
}

/// <summary>ProjectDocument = a document scoped to one Project; OrgWiki = an org-wide wiki page (no ProjectId).</summary>
public enum DocScope
{
    ProjectDocument,
    OrgWiki
}

/// <summary>Only meaningful when DocPage.Scope == OrgWiki.</summary>
public enum DocCategory
{
    KnowledgeBase,
    ApiDocs,
    MeetingNotes,
    Architecture,
    ReleaseNotes,
    General
}

// ---------------------------------------------------------------------------
// Phase 5: Reports / Dashboards / Analytics
// ---------------------------------------------------------------------------

/// <summary>Traffic-light health score computed identically by the executive dashboard's per-project
/// summary and the single-project health endpoint (see ReportsHealthScorer) so the two can never
/// silently diverge.</summary>
public enum ProjectHealthStatus
{
    Green,
    Yellow,
    Red
}

/// <summary>Grouping granularity for the productivity report's time buckets.</summary>
public enum ProductivityBucket
{
    Week,
    Month
}

// ---------------------------------------------------------------------------
// Phase 6: AI Features / Workflow Automation
// ---------------------------------------------------------------------------

public enum AiChatRole
{
    User,
    Assistant
}

/// <summary>What kind of application event a WorkflowDefinition fires on. Scheduled workflows have
/// no triggering WorkItem — they execute their actions unconditionally on a cron cadence.</summary>
public enum WorkflowTriggerType
{
    WorkItemCreated,
    WorkItemStatusChanged,
    WorkItemAssigned,
    SprintStarted,
    Scheduled
}

/// <summary>Comparison operator for a single WorkflowCondition. All conditions on a
/// WorkflowDefinition are ANDed together — no nested groups, deliberately kept simple.</summary>
public enum WorkflowConditionOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    Contains
}

public enum WorkflowActionType
{
    ChangeStatus,
    AssignUser,
    AddLabel,
    SendNotification,
    CallWebhook,
    RequireApproval
}

public enum WorkflowRunStatus
{
    Running,
    Succeeded,
    Failed,
    AwaitingApproval
}

public enum WorkflowApprovalStatus
{
    Pending,
    Approved,
    Rejected
}
