namespace FoodOrder.Application.DTOs.Branch;

public class BranchDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateBranchDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }

    /// <summary>SuperAdmin only — Admins always create a branch for their own organization.</summary>
    public int? OrganizationId { get; set; }
}

public class UpdateBranchDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Per-branch revenue/orders breakdown — the "compare my branches" view.</summary>
public class BranchReportDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    public int ThisMonthOrders { get; set; }
    public decimal ThisMonthRevenue { get; set; }
}
