using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Application.DTOs;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, string? AvatarUrl,
    bool IsEmailVerified, bool TwoFactorEnabled, UserStatus Status, DateTime? LastLoginAt,
    Guid? OrganizationId, DateTime CreatedAt, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);

public record CurrentUserDto(Guid Id, string Email, string FirstName, string LastName, string? AvatarUrl,
    Guid? OrganizationId, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);

public record AuthResultDto(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, UserDto User);

public record OrganizationDto(Guid Id, string Name, string Slug, string? LogoUrl, string? Domain,
    SubscriptionPlan SubscriptionPlan, bool IsActive, DateTime CreatedAt);

public record DepartmentDto(Guid Id, Guid OrganizationId, string Name, string? Description, Guid? ParentDepartmentId);

public record TeamDto(Guid Id, Guid OrganizationId, Guid? DepartmentId, string Name, string? Description, int MemberCount);

public record TeamMemberDto(Guid Id, Guid TeamId, Guid UserId, string RoleInTeam, string? UserEmail, string? UserFullName);

public record RoleDto(Guid Id, Guid? OrganizationId, string Name, bool IsSystemRole, IReadOnlyList<string> Permissions);

public record PermissionDto(Guid Id, string Key, string? Description, string Category);

public record InvitationDto(Guid Id, Guid OrganizationId, string Email, Guid RoleId, string RoleName,
    InvitationStatus Status, DateTime ExpiresAt, DateTime CreatedAt);

public record AuditLogDto(Guid Id, Guid? OrganizationId, Guid? UserId, string Action, string EntityType,
    string? EntityId, string? MetadataJson, string? IpAddress, DateTime CreatedAt);

// ---------------------------------------------------------------------------
// Phase 2: Projects, Milestones, Labels, WorkItems/Kanban
// ---------------------------------------------------------------------------

public record WorkItemStatusCountDto(WorkItemStatus Status, int Count);

public record ProjectDto(Guid Id, Guid OrganizationId, string Key, string Name, string? Description,
    ProjectStatus Status, DateTime? StartDate, DateTime? EndDate, Guid OwnerUserId, bool IsArchived,
    DateTime CreatedAt, int MemberCount, IReadOnlyList<WorkItemStatusCountDto> WorkItemCounts);

public record ProjectMemberDto(Guid Id, Guid ProjectId, Guid UserId, string RoleInProject,
    string? UserEmail, string? UserFullName);

public record MilestoneDto(Guid Id, Guid ProjectId, string Name, string? Description,
    DateTime? DueDate, MilestoneStatus Status, DateTime CreatedAt);

public record LabelDto(Guid Id, Guid ProjectId, string Name, string ColorHex);

public record WorkItemDto(Guid Id, Guid ProjectId, Guid? ParentWorkItemId, string Title, string? Description,
    WorkItemStatus Status, WorkItemPriority Priority, WorkItemType Type, int? StoryPoints, decimal? EstimatedHours,
    Guid? AssigneeUserId, string? AssigneeFullName, Guid ReporterUserId, string? ReporterFullName,
    DateTime? DueDate, bool IsRecurring, int? RecurrenceIntervalDays, double Position,
    DateTime CreatedAt, DateTime? UpdatedAt, IReadOnlyList<LabelDto> Labels);

public record KanbanColumnDto(WorkItemStatus Status, IReadOnlyList<WorkItemDto> Items);

public record KanbanBoardDto(Guid ProjectId, IReadOnlyList<KanbanColumnDto> Columns);

public record ChecklistItemDto(Guid Id, Guid WorkItemId, string Text, bool IsDone, int Position);

public record WorkItemDependencyDto(Guid Id, Guid WorkItemId, Guid DependsOnWorkItemId,
    WorkItemDependencyType DependencyType, string DependsOnTitle, WorkItemStatus DependsOnStatus);

public record WorkItemCommentDto(Guid Id, Guid WorkItemId, Guid AuthorUserId, string? AuthorFullName,
    string Body, DateTime CreatedAt, DateTime? UpdatedAt, IReadOnlyList<Guid> MentionedUserIds);

public record WorkItemAttachmentDto(Guid Id, Guid WorkItemId, string FileName, long FileSizeBytes,
    Guid UploadedByUserId, DateTime CreatedAt, string DownloadUrl);

public record WorkItemActivityDto(Guid Id, Guid WorkItemId, Guid UserId, string Action,
    string? FieldName, string? OldValue, string? NewValue, DateTime CreatedAt);

public record WorkItemTimeLogDto(Guid Id, Guid WorkItemId, Guid UserId, int Minutes, string? Note,
    DateOnly LoggedDate, bool IsBillable, DateTime CreatedAt);

public record CustomFieldDefinitionDto(Guid Id, Guid ProjectId, string Name, CustomFieldType FieldType,
    string? OptionsJson, bool IsRequired);

public record CustomFieldValueDto(Guid Id, Guid WorkItemId, Guid CustomFieldDefinitionId, string ValueJson);

public record WorkItemDetailDto(Guid Id, Guid ProjectId, Guid? ParentWorkItemId, string Title, string? Description,
    WorkItemStatus Status, WorkItemPriority Priority, WorkItemType Type, int? StoryPoints, decimal? EstimatedHours,
    Guid? AssigneeUserId, string? AssigneeFullName, Guid ReporterUserId, string? ReporterFullName,
    DateTime? DueDate, bool IsRecurring, int? RecurrenceIntervalDays, double Position,
    DateTime CreatedAt, DateTime? UpdatedAt,
    IReadOnlyList<LabelDto> Labels, IReadOnlyList<ChecklistItemDto> ChecklistItems,
    IReadOnlyList<Guid> FollowerUserIds, IReadOnlyList<WorkItemDependencyDto> Dependencies,
    IReadOnlyList<WorkItemCommentDto> Comments, IReadOnlyList<WorkItemAttachmentDto> Attachments,
    IReadOnlyList<WorkItemActivityDto> Activities, IReadOnlyList<WorkItemTimeLogDto> TimeLogs,
    decimal ActualHours, IReadOnlyList<CustomFieldValueDto> CustomFieldValues);

// ---------------------------------------------------------------------------
// Phase 3: Scrum, Gantt, Calendar, Time Tracking
// ---------------------------------------------------------------------------

public record SprintDto(Guid Id, Guid ProjectId, string Name, string? Goal, DateTime StartDate, DateTime EndDate,
    SprintStatus Status, int WorkItemCount, int CompletedWorkItemCount, int TotalStoryPoints,
    int CompletedStoryPoints, DateTime CreatedAt);

/// <summary>Same shape as Phase 2's KanbanColumnDto/KanbanBoardDto, reused verbatim for the Sprint board.</summary>
public record SprintBoardDto(Guid SprintId, IReadOnlyList<KanbanColumnDto> Columns);

public record VelocitySprintDto(Guid SprintId, string SprintName, int CommittedPoints, int CompletedPoints);
public record VelocityDto(IReadOnlyList<VelocitySprintDto> Sprints);

public record BurndownPointDto(DateOnly Date, int RemainingPoints, double IdealRemainingPoints);
public record BurndownDto(IReadOnlyList<BurndownPointDto> Days);

public record BurnupPointDto(DateOnly Date, int CompletedPoints, int TotalScopePoints);
public record BurnupDto(IReadOnlyList<BurnupPointDto> Days);

public record RetrospectiveNoteDto(Guid Id, Guid SprintId, RetrospectiveCategory Category, string Text,
    Guid CreatedByUserId, string? CreatedByName, DateTime CreatedAt);
public record RetrospectiveDto(IReadOnlyList<RetrospectiveNoteDto> Notes);

public record GanttDependencyDto(Guid DependsOnWorkItemId, WorkItemDependencyType DependencyType);

public record GanttItemDto(Guid Id, string Title, DateTime StartDate, DateTime EndDate, double Progress,
    WorkItemStatus Status, WorkItemPriority Priority, Guid? ParentWorkItemId, IReadOnlyList<GanttDependencyDto> Dependencies);

public record GanttMilestoneDto(Guid Id, string Name, DateTime? DueDate);

public record GanttChartDto(IReadOnlyList<GanttItemDto> Items, IReadOnlyList<GanttMilestoneDto> Milestones);

public record CriticalPathDto(IReadOnlyList<Guid> WorkItemIds);

public record GanttBaselineDto(Guid Id, Guid ProjectId, string Name, DateTime CreatedAt, int ItemCount);

public record GanttBaselineItemDto(Guid WorkItemId, string Title, DateTime PlannedStartDate, DateTime PlannedEndDate);

public record GanttBaselineDetailDto(Guid Id, string Name, DateTime CreatedAt, IReadOnlyList<GanttBaselineItemDto> Items);

public record CalendarEventDto(Guid Id, CalendarEventType Type, string Title, DateTime Date, DateTime? EndDate,
    Guid? ProjectId, string? ProjectName);
public record CalendarResultDto(IReadOnlyList<CalendarEventDto> Events);

public record WorkloadRowDto(Guid UserId, string UserName, DateOnly Date, decimal AllocatedHours);
public record WorkloadResultDto(IReadOnlyList<WorkloadRowDto> Rows);

public record ActiveTimerDto(Guid Id, Guid WorkItemId, string WorkItemTitle, DateTime StartedAt);

public record TimesheetEntryDto(Guid Id, DateOnly Date, Guid WorkItemId, string WorkItemTitle, string ProjectName,
    int Minutes, bool IsBillable);
public record TimesheetDto(IReadOnlyList<TimesheetEntryDto> Entries, int TotalMinutes, int BillableMinutes);

// ---------------------------------------------------------------------------
// Phase 4: Chat, Notifications, Documents/Wiki
// ---------------------------------------------------------------------------

public record ChatChannelDto(Guid Id, Guid OrganizationId, Guid? ProjectId, string Name, bool IsPrivate,
    int MemberCount, int UnreadCount, DateTime CreatedAt);

public record MessageReactionGroupDto(string Emoji, IReadOnlyList<Guid> UserIds);

public record MessageAttachmentDto(Guid Id, string FileName, string DownloadUrl, long FileSizeBytes);

public record ChatMessageDto(Guid Id, Guid? ChannelId, Guid? DirectConversationId, Guid AuthorUserId,
    string? AuthorName, string Body, Guid? ParentMessageId, DateTime CreatedAt, DateTime? EditedAt, bool IsDeleted,
    IReadOnlyList<MessageReactionGroupDto> Reactions, IReadOnlyList<MessageAttachmentDto> Attachments, int ReplyCount);

public record ChatMessageListDto(IReadOnlyList<ChatMessageDto> Messages);

public record DirectConversationDto(Guid Id, Guid OtherUserId, string OtherUserName, bool OtherUserOnline,
    string? LastMessagePreview, DateTime? LastMessageAt, int UnreadCount);

public record NotificationDto(Guid Id, NotificationType Type, string Title, string Body, string? LinkUrl,
    bool IsRead, DateTime CreatedAt);

public record UnreadCountDto(int Count);

public record NotificationPreferenceDto(NotificationChannel Channel, bool IsEnabled);

public record OrganizationIntegrationSettingsDto(string? SlackWebhookUrl, string? TeamsWebhookUrl, string? SmsProviderUrl);

public record DocPageSummaryDto(Guid Id, string Title, DocScope Scope, DocCategory? Category,
    Guid? ParentPageId, DateTime UpdatedAt, string? UpdatedByName);

public record DocPageDetailDto(Guid Id, Guid OrganizationId, Guid? ProjectId, DocScope Scope, DocCategory? Category,
    string Title, string Content, Guid? ParentPageId, Guid CreatedByUserId, string? CreatedByName,
    string? UpdatedByName, DateTime CreatedAt, DateTime? UpdatedAt, int VersionCount);

public record DocPageVersionSummaryDto(Guid Id, int VersionNumber, string? EditedByName, DateTime CreatedAt);

public record DocPageVersionDetailDto(int VersionNumber, string Content, string? EditedByName, DateTime CreatedAt);

public record DocPageCommentDto(Guid Id, Guid AuthorUserId, string? AuthorName, string Body, DateTime CreatedAt);

public record ImageUploadResultDto(string Url);

// ---------------------------------------------------------------------------
// Phase 5: Reports / Dashboards / Analytics
// ---------------------------------------------------------------------------

/// <summary>WorkItemDto is reused verbatim as the Kanban "card" shape (per Phase 2), so the
/// dashboard's todaysTasks/overdueTasks and the sprint report's blockedItems just return
/// IReadOnlyList&lt;WorkItemDto&gt; rather than a brand-new "WorkItemCardDto".</summary>
public record RecentActivityItemDto(Guid Id, string Message, string? LinkUrl, DateTime CreatedAt);

public record ActiveSprintSummaryDto(Guid SprintId, string SprintName, Guid ProjectId, string ProjectName,
    double ProgressPercent, int DaysRemaining);

public record DashboardDto(IReadOnlyList<WorkItemDto> TodaysTasks, IReadOnlyList<WorkItemDto> OverdueTasks,
    int AssignedTaskCount, IReadOnlyList<RecentActivityItemDto> RecentActivity,
    IReadOnlyList<ActiveSprintSummaryDto> ActiveSprints, int UnreadNotificationCount);

public record ProjectHealthSummaryDto(Guid ProjectId, string ProjectName, ProjectHealthStatus Health,
    double ProgressPercent, int OverdueCount, string? ActiveSprintName);

public record ExecutiveDashboardDto(int TotalProjects, int ActiveProjects, int TotalWorkItems,
    int CompletedThisMonth, int OverdueCount, int AtRiskProjectCount,
    IReadOnlyList<ProjectHealthSummaryDto> ProjectHealthSummaries);

public record SprintDashboardDto(SprintDto Sprint, BurndownDto Burndown,
    IReadOnlyList<WorkItemDto> BlockedItems, int OpenRetroActionItemCount);

/// <summary>CycleTimeHours is null when the item never recorded an InProgress (or other
/// work-started) transition — see GetCycleTimeReportQueryHandler's documented judgment call.</summary>
public record CycleTimeWorkItemDto(Guid WorkItemId, string Title, double LeadTimeHours, double? CycleTimeHours);

public record CycleTimeReportDto(double? AverageLeadTimeHours, double? AverageCycleTimeHours,
    IReadOnlyList<CycleTimeWorkItemDto> Items);

public record ProjectHealthDto(ProjectHealthStatus Status, IReadOnlyList<string> Reasons,
    int OverdueCount, int BlockedCount, double? SprintProgressPercent);

public record ProductivityBucketDto(DateOnly PeriodStart, int CompletedCount, int CompletedPoints);

public record ProductivityReportDto(IReadOnlyList<ProductivityBucketDto> Buckets);

public record ResourceUtilizationRowDto(Guid UserId, string UserName, decimal EstimatedHours,
    decimal LoggedHours, int TaskCount, double? UtilizationPercent);

public record ResourceUtilizationReportDto(IReadOnlyList<ResourceUtilizationRowDto> Rows);

public record CostAnalysisUserRowDto(Guid UserId, string UserName, decimal BillableHours, decimal Cost);

public record CostAnalysisReportDto(decimal TotalBillableHours, decimal TotalNonBillableHours,
    decimal TotalCost, bool HourlyRateConfigured, IReadOnlyList<CostAnalysisUserRowDto> ByUser);

// ---------------------------------------------------------------------------
// Phase 6: AI Features
// ---------------------------------------------------------------------------

public record TaskSuggestionDto(string Title, string Description, WorkItemPriority Priority, WorkItemType Type, int? StoryPoints);
public record GenerateTasksResultDto(IReadOnlyList<TaskSuggestionDto> Suggestions);

public record PlanSprintResultDto(IReadOnlyList<Guid> SelectedWorkItemIds, string Reasoning);

public record AnalyzeBugResultDto(string ProbableRootCause, string ReproSteps, string SuggestedSeverity, string Reasoning);

public record ActionItemDto(string Text);
public record SummarizeMeetingResultDto(string Summary, IReadOnlyList<ActionItemDto> ActionItems, Guid? WikiPageId);

public record ChatResultDto(Guid ConversationId, string Reply);
public record AiChatConversationSummaryDto(Guid Id, string Title, DateTime CreatedAt, DateTime LastMessageAt);
public record AiChatMessageDto(Guid Id, AiChatRole Role, string Content, DateTime CreatedAt);

public record RiskPredictionDto(string RiskLevel, string Explanation);

public record ResourceAllocationSuggestionDto(Guid UserId, string UserName, string Recommendation);
public record ResourceAllocationResultDto(IReadOnlyList<ResourceAllocationSuggestionDto> Suggestions, string Reasoning);

public record EstimateStoryPointsResultDto(int SuggestedPoints, string Reasoning);

public record PredictDeadlineResultDto(DateTime PredictedDate, string Confidence, string Reasoning);

public record PrioritizedTaskDto(Guid WorkItemId, string SuggestedPriority, string Reasoning);
public record PrioritizeTasksResultDto(IReadOnlyList<PrioritizedTaskDto> Items);

public record CodeReviewSuggestionDto(int? Line, string Comment, string Severity);
public record ReviewCodeResultDto(IReadOnlyList<CodeReviewSuggestionDto> Suggestions, string Summary);

public record GenerateReleaseNotesResultDto(string Markdown, Guid? WikiPageId);

// ---------------------------------------------------------------------------
// Phase 6: Workflow Automation
// ---------------------------------------------------------------------------

public record WorkflowSummaryDto(Guid Id, string Name, WorkflowTriggerType TriggerType, bool IsEnabled,
    int ConditionCount, int ActionCount, DateTime CreatedAt);

public record WorkflowConditionDto(Guid Id, string FieldPath, WorkflowConditionOperator Operator, string Value);
public record WorkflowActionDto(Guid Id, WorkflowActionType ActionType, string ActionConfigJson, int Order);

public record WorkflowDetailDto(Guid Id, Guid OrganizationId, Guid? ProjectId, string Name,
    WorkflowTriggerType TriggerType, string TriggerConfigJson, bool IsEnabled, DateTime CreatedAt,
    IReadOnlyList<WorkflowConditionDto> Conditions, IReadOnlyList<WorkflowActionDto> Actions);

public record WorkflowRunDto(Guid Id, WorkflowRunStatus Status, Guid? TriggerEntityId,
    DateTime StartedAt, DateTime? CompletedAt, string LogJson);

public record ApprovalSummaryDto(Guid Id, Guid WorkflowRunId, string WorkflowName, WorkflowApprovalStatus Status, DateTime RequestedAt);
