using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class RolePermissionService : IRolePermissionService
{
    private readonly IRolePermissionRepository _repo;
    private readonly IRoleRepository _roleRepo;

    public RolePermissionService(IRolePermissionRepository repo, IRoleRepository roleRepo)
    {
        _repo = repo;
        _roleRepo = roleRepo;
    }

    public Task<List<string>> GetByRoleIdAsync(int roleId)
        => _repo.GetByRoleIdAsync(roleId);

    public async Task SaveForRoleAsync(int roleId, IEnumerable<string> features)
    {
        var role = await _roleRepo.GetByIdAsync(roleId)
            ?? throw new AppException("Role not found.", statusCode: 404);

        await _repo.SaveForRoleAsync(roleId, features);
    }
}
