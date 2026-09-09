namespace FoodOrder.Application.DTOs.User;

public class UpdateStaffUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }

    /// <summary>Which branch this staff member is scoped to. Null = org-wide (sees every branch combined).</summary>
    public int? BranchId { get; set; }
    public string? ProfileImage { get; set; }
}
