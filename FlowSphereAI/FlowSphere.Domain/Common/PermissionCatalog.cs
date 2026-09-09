namespace FlowSphere.Domain.Common;

/// <summary>Every permission key this module recognizes - each becomes an authorization policy
/// registered in Program.cs, mirroring the PermissionCatalog convention used by NovaERP.</summary>
public static class PermissionCatalog
{
    public const string WorkflowsRead = "workflows.read";
    public const string WorkflowsWrite = "workflows.write";
    public const string WorkflowsPublish = "workflows.publish";
    public const string WorkflowsExecute = "workflows.execute";

    /// <summary>Resolves a pending UserTask (accept/reject) step. Deliberately separate from
    /// WorkflowsExecute so a narrowly-scoped role (e.g. "HR") can review/decide approvals without
    /// also being able to manually trigger arbitrary workflow runs.</summary>
    public const string WorkflowsApprove = "workflows.approve";
    public const string ExecutionsRead = "executions.read";
    public const string ConnectorsManage = "connectors.manage";
    public const string UsersManage = "users.manage";
    public const string BillingManage = "billing.manage";

    /// <summary>Changes platform-wide preferences (branding/background image today) - org-admin-
    /// only, same convention as BillingManage/UsersManage/ConnectorsManage below.</summary>
    public const string SettingsManage = "settings.manage";
    public const string WorkspacesRead = "workspaces.read";
    public const string WorkspacesWrite = "workspaces.write";
    public const string AppsRead = "apps.read";
    public const string AppsWrite = "apps.write";

    /// <summary>Fills out and submits a published app's form. Deliberately separate from
    /// AppsWrite so a narrowly-scoped role (e.g. "Employee") can use an app without also being
    /// able to edit/delete it.</summary>
    public const string AppsSubmit = "apps.submit";
    public const string TablesRead = "tables.read";
    public const string TablesWrite = "tables.write";

    /// <summary>Permissions granted to a newly registered organization's Admin role.</summary>
    public static readonly string[] All =
    {
        WorkflowsRead, WorkflowsWrite, WorkflowsPublish, WorkflowsExecute, WorkflowsApprove, ExecutionsRead, ConnectorsManage, UsersManage, BillingManage, SettingsManage,
        WorkspacesRead, WorkspacesWrite, AppsRead, AppsWrite, AppsSubmit, TablesRead, TablesWrite
    };

    /// <summary>Reduced permission set for non-admin team members invited into an existing
    /// organization - can build/run workflows but can't manage connectors or other users.</summary>
    public static readonly string[] Member =
    {
        WorkflowsRead, WorkflowsWrite, WorkflowsPublish, WorkflowsExecute, WorkflowsApprove, ExecutionsRead,
        WorkspacesRead, WorkspacesWrite, AppsRead, AppsWrite, AppsSubmit, TablesRead, TablesWrite
    };

    /// <summary>Reviews and resolves pending approvals (e.g. leave requests), and submits HR-side
    /// forms (e.g. onboarding a new employee) - can see workspaces, apps, and workflows, but
    /// can't build/edit them or trigger runs directly.</summary>
    public static readonly string[] Hr =
    {
        WorkspacesRead, AppsRead, AppsSubmit, WorkflowsRead, WorkflowsApprove, ExecutionsRead, TablesRead,
    };

    /// <summary>Fills out and submits published apps - can't edit apps, tables, or workflows.</summary>
    public static readonly string[] Employee =
    {
        WorkspacesRead, AppsRead, AppsSubmit,
    };

    /// <summary>Keys a WorkspaceRole's permission checklist may draw from - the subset of
    /// PermissionCatalog that's meaningful scoped to a single workspace (org-wide-only keys like
    /// billing.manage/users.manage/connectors.manage are excluded).</summary>
    public static readonly string[] WorkspaceScoped =
    {
        WorkspacesRead, WorkspacesWrite, AppsRead, AppsWrite, AppsSubmit, TablesRead, TablesWrite
    };

    /// <summary>Resolves a role name (as stored on a Role row) to the permission set it should
    /// get when the role doesn't already exist and needs to be created. "Admin" and the built-in
    /// roles below get their own tailored set; any other name falls back to Member.</summary>
    public static string[] PermissionsForRoleName(string roleName) => roleName switch
    {
        "Admin" => All,
        "HR" => Hr,
        "Employee" => Employee,
        _ => Member,
    };
}
