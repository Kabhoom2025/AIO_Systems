namespace FoodOrder.Application.DTOs.Auth;

public class LoginResponseDto
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public bool IsSuperAdmin { get; set; }
    public string? ProfileImage { get; set; }
    public List<string> EnabledModules { get; set; } = new();
}
