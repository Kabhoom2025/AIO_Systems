namespace ProjectFlowAI.Application.Common;

public record PermissionDefinition(string Key, string Category, string Description);

/// <summary>
/// Single source of truth for permission keys, mirroring NovaERP.Application.Common.PermissionCatalog:
/// shared by ASP.NET Core policy registration (Program.cs) and Infrastructure seed data.
/// Phase 1 only implements Organizations/Departments/Teams/Users/Roles/Invitations/AuditLogs —
/// "projects.manage" and friends are reserved keys for later phases to build on.
/// </summary>
public static class PermissionCatalog
{
    public const string OrganizationsManage = "organizations.manage";
    public const string OrganizationsView = "organizations.view";
    public const string DepartmentsManage = "departments.manage";
    public const string DepartmentsView = "departments.view";
    public const string TeamsManage = "teams.manage";
    public const string TeamsView = "teams.view";
    public const string UsersInvite = "users.invite";
    public const string UsersManage = "users.manage";
    public const string UsersView = "users.view";
    public const string RolesManage = "roles.manage";
    public const string RolesView = "roles.view";
    public const string AuditLogsView = "audit-logs.view";

    // Phase 2: Projects/Milestones/WorkItems/Kanban. Un-reserved now that the controllers exist.
    public const string ProjectsManage = "projects.manage";
    public const string ProjectsView = "projects.view";
    public const string TasksManage = "tasks.manage";
    public const string TasksView = "tasks.view";
    public const string MilestonesManage = "milestones.manage";
    public const string CommentsManage = "comments.manage";
    public const string ReportsView = "reports.view";

    // Phase 3: Scrum/Gantt/Calendar/Time Tracking. Reuses TasksView/ProjectsView for read-only
    // Gantt/Calendar projections (they're just alternate views over WorkItems/Projects), but gets
    // its own manage/view pair for Sprint lifecycle actions since "who can start/complete a sprint"
    // is a distinct concern from "who can edit a work item".
    public const string SprintsManage = "sprints.manage";
    public const string SprintsView = "sprints.view";

    // Phase 4: Chat/Notifications/Documents/Wiki. Chat access itself is gated by channel
    // membership (checked in the command handler), so "chat.use" just means "can use chat at
    // all" — everyone gets it. "notifications.manage" is for org-level integration settings
    // (Slack/Teams/SMS webhooks), not per-user notification preferences (those need no permission
    // beyond being authenticated, since a user always manages their own).
    public const string ChatUse = "chat.use";
    public const string NotificationsManage = "notifications.manage";
    public const string DocsManage = "docs.manage";
    public const string DocsView = "docs.view";

    // Phase 6: AI Features / Workflow Automation. "ai.use" is intentionally broad — everyone with
    // an org membership can use AI features, no fine-grained role gating. "automation.manage" gates
    // creating/editing/enabling/disabling workflows (PM/TeamLead/OrgAdmin); approval decisions are
    // NOT gated by a permission at all — they're gated by "you are the RequestedApproverUserId on
    // this specific approval", checked inside the ApproveOrRejectCommand handler.
    public const string AiUse = "ai.use";
    public const string AutomationManage = "automation.manage";

    public static IReadOnlyList<PermissionDefinition> All { get; } = new List<PermissionDefinition>
    {
        new(OrganizationsManage, "Organizations", "Create/update/deactivate organizations"),
        new(OrganizationsView, "Organizations", "View organization details"),
        new(DepartmentsManage, "Departments", "Create/update/delete departments"),
        new(DepartmentsView, "Departments", "View departments"),
        new(TeamsManage, "Teams", "Create/update teams and manage membership"),
        new(TeamsView, "Teams", "View teams"),
        new(UsersInvite, "Users", "Invite new users into an organization"),
        new(UsersManage, "Users", "Manage user accounts (suspend/roles/etc.)"),
        new(UsersView, "Users", "View user accounts"),
        new(RolesManage, "Roles", "Create roles and assign/revoke roles"),
        new(RolesView, "Roles", "View roles and permissions"),
        new(AuditLogsView, "AuditLogs", "View the audit log"),
        new(ProjectsManage, "Projects", "Create/update/archive projects and manage membership"),
        new(ProjectsView, "Projects", "View projects"),
        new(TasksManage, "Tasks", "Create/update/move work items, checklists, labels, dependencies, attachments"),
        new(TasksView, "Tasks", "View work items and the Kanban board"),
        new(MilestonesManage, "Milestones", "Create/update/delete project milestones"),
        new(CommentsManage, "Comments", "Create/edit/delete comments (own comments, or any as org admin)"),
        new(ReportsView, "Reports", "View executive/sprint/cycle-time/productivity/utilization/cost reports"),
        new(SprintsManage, "Sprints", "Create/start/complete sprints, manage retrospectives and Gantt baselines"),
        new(SprintsView, "Sprints", "View sprints, the sprint board, burndown/burnup, Gantt, calendar and timesheets"),
        new(ChatUse, "Chat", "Use chat: view/send in channels and direct messages you're a member of"),
        new(NotificationsManage, "Notifications", "Configure org-level Slack/Teams/SMS integration settings"),
        new(DocsManage, "Documents", "Create/edit/delete project documents and org wiki pages"),
        new(DocsView, "Documents", "View project documents and org wiki pages"),
        new(AiUse, "AI", "Use AI features: task generation, sprint planning, chat, predictions, code review"),
        new(AutomationManage, "Automation", "Create/edit/enable/disable workflow automations"),
    };
}
