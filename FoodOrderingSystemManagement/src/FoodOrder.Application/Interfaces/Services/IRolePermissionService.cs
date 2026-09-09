namespace FoodOrder.Application.Interfaces.Services;

public interface IRolePermissionService
{
    Task<List<string>> GetByRoleIdAsync(int roleId);
    Task SaveForRoleAsync(int roleId, IEnumerable<string> features);
}
