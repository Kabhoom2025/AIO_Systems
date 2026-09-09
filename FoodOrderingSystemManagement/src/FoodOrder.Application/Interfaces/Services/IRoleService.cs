using FoodOrder.Application.DTOs.Role;

namespace FoodOrder.Application.Interfaces.Services;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(int? organizationId = null);
    Task<RoleDto>                CreateAsync(CreateRoleDto dto);
}
