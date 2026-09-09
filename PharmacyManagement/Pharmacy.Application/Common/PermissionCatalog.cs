namespace Pharmacy.Application.Common;

public record PermissionDefinition(string Key, string Module, string Description);

/// <summary>Single source of truth for permission keys, shared by policy registration and seeding.</summary>
public static class PermissionCatalog
{
    private static readonly string[] Modules =
    {
        "Medicines", "Prescriptions", "Suppliers", "PurchaseOrders",
        "GoodsReceipts", "StockAdjustments", "Branches", "Roles",
        "Doctors", "Patients", "Customers", "Sales", "Expenses", "Reports",
        "Deliveries", "Users", "Settings", "AuditLogs", "Appointments"
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

        list.Add(new PermissionDefinition("purchase-orders.approve", "PurchaseOrders", "Approve / send purchase orders"));
        return list;
    }

    private static string ToKeySegment(string module) => module switch
    {
        "PurchaseOrders" => "purchase-orders",
        "GoodsReceipts" => "goods-receipts",
        "StockAdjustments" => "stock-adjustments",
        "AuditLogs" => "audit-logs",
        _ => module.ToLowerInvariant()
    };
}
