namespace FoodOrder.Application.DTOs.Role;

public class RoleDto
{
    public int    Id        { get; set; }
    public string RoleName  { get; set; } = string.Empty;
    public int    UserCount { get; set; }
}
