namespace NovaERP.Application.Common;

public record PermissionDefinition(string Key, string Module, string Description);

/// <summary>
/// Single source of truth for permission keys, shared by policy registration and seeding.
/// Includes catalog-only entries for the full eventual ERP module list — only the
/// foundation modules (Organization, Branches, Departments, Users, Roles, Notifications,
/// AuditLogs, Settings, Documents) have real controllers in this phase; the rest are
/// reserved keys so roles/permissions can be assigned ahead of each module's build-out.
/// </summary>
public static class PermissionCatalog
{
    private static readonly string[] Modules =
    {
        // Foundation (this phase has controllers)
        "Organization", "Branches", "Departments", "Users", "Roles",
        "Notifications", "AuditLogs", "Settings", "Documents",

        // Phase 2 — Approval Workflow Engine + Workflow Automation (this phase has controllers)
        "Workflows", "Automation",

        // Phase 2 — Scheduler (Hangfire-backed recurring jobs, this phase has a controller)
        "Scheduler",

        // Phase 3 — Tax Engine (this phase has a controller)
        "Tax",

        // Phase 3 — CRM: Leads, Accounts, Contacts, Opportunities (this phase has controllers)
        "CRM",

        // Phase 3 — Sales: Sales Orders (this phase has a controller)
        "Sales",

        // Phase 3 — Procurement: Vendors, RFQs (this phase has controllers)
        "Procurement",

        // Phase 3 — Purchase: Purchase Orders (this phase has a controller)
        "Purchase",

        // Phase 4 — Inventory: Products, Stock Movements (this phase has controllers)
        "Inventory",

        // Phase 4 — Warehouse: Warehouses, Stock Transfers (this phase has controllers)
        "Warehouse",

        // Phase 4 — Manufacturing: Bills of Materials, Production Orders (this phase has controllers)
        "Manufacturing",

        // Phase 4 addendum — Logistics: Vehicles, Delivery Loads (this phase has controllers)
        "Logistics",

        // Phase 4 addendum — Carrier Connector: external carrier/TMS rate-shop integrations
        // (this phase has a controller)
        "CarrierConnector",

        // Phase 5 — General Ledger: Ledger Accounts, Journal Entries (this phase has controllers)
        "Finance",

        // Reserved for future ERP modules (catalog-only, no controllers yet)
        "HRMS", "Payroll", "Projects", "Assets", "ServiceDesk",
        "POS", "Retail", "Reports"
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

        return list;
    }

    private static string ToKeySegment(string module) => module switch
    {
        "AuditLogs"       => "audit-logs",
        "ServiceDesk"     => "service-desk",
        "CarrierConnector" => "carrier-connector",
        _ => module.ToLowerInvariant()
    };
}
