namespace FoodOrder.Application.Interfaces.Repositories;

public interface IRolePermissionRepository
{
    Task<List<string>> GetByRoleIdAsync(int roleId);
    Task SaveForRoleAsync(int roleId, IEnumerable<string> features);
}
