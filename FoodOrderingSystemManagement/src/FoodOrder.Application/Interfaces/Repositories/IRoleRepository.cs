using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<IReadOnlyList<Role>> GetAllWithUsersAsync(int? organizationId = null);
    Task<Role?> GetByNameAsync(string roleName);
}
