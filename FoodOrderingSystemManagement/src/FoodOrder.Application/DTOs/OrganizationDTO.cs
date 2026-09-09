namespace FoodOrder.Application.DTOs;

public class OrganizationDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public string? Timezone { get; set; }
    public string? Currency { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoUrl { get; set; }
    public string? Timezone { get; set; }
    public string? Currency { get; set; } = "INR";
}

public class UpdateOrganizationRequest : CreateOrganizationRequest
{
    public bool IsActive { get; set; } = true;
}

public class OrgUserDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateOrgAdminRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Null = org-wide Admin (sees every branch). Set to scope this admin to one branch.</summary>
    public int? BranchId { get; set; }
}

public class OrgReportDto
{
    public int OrganizationId { get; set; }
    public string OrgName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveUsers { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    public int ThisMonthOrders { get; set; }
    public decimal ThisMonthRevenue { get; set; }
}
