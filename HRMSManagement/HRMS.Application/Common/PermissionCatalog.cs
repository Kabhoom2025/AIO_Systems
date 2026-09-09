namespace HRMS.Application.Common;

public record PermissionDefinition(string Key, string Module, string Description);

/// <summary>Single source of truth for permission keys, shared by policy registration and seeding.</summary>
public static class PermissionCatalog
{
    private static readonly string[] Modules =
    {
        "Organization", "Branches", "Departments", "Designations", "Shifts",
        "Holidays", "Employees", "Attendance", "Leaves", "Payroll",
        "Recruitment", "Performance", "Assets", "Expenses", "HelpDesk",
        "Users", "Roles", "Reports", "AuditLogs", "Settings", "Workflows"
    };

    private static readonly string[] Actions = { "view", "create", "edit", "delete" };

    public static IReadOnlyList<PermissionDefinition> All { get; } = BuildCatalog();

    private static IReadOnlyList<PermissionDefinition> BuildCatalog()
    {
        var list = new List<PermissionDefinition>();
        foreach (var module in Modules)
        {
            var moduleKey = ToKeySegment(module);
            foreach (var action in Actions)
                list.Add(new PermissionDefinition($"{moduleKey}.{action}", module, $"{action} {module}"));
        }

        list.Add(new PermissionDefinition("attendance.approve", "Attendance", "Approve attendance regularizations"));
        list.Add(new PermissionDefinition("leaves.approve", "Leaves", "Approve / reject leave requests"));
        list.Add(new PermissionDefinition("expenses.approve", "Expenses", "Approve / reject expense claims"));
        list.Add(new PermissionDefinition("payroll.process", "Payroll", "Process payroll runs"));
        list.Add(new PermissionDefinition("payroll.lock", "Payroll", "Lock / unlock payroll runs"));
        list.Add(new PermissionDefinition("performance.review", "Performance", "Complete manager reviews"));
        return list;
    }

    private static string ToKeySegment(string module) => module switch
    {
        "HelpDesk"  => "helpdesk",
        "AuditLogs" => "audit-logs",
        _ => module.ToLowerInvariant()
    };
}
