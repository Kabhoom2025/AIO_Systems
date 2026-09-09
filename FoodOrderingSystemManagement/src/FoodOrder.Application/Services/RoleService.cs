using FoodOrder.Application.DTOs.Role;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(int? organizationId = null)
    {
        var roles = await _roleRepository.GetAllWithUsersAsync(organizationId);
        return roles.Select(r => new RoleDto
        {
            Id        = r.Id,
            RoleName  = r.RoleName,
            UserCount = r.Users.Count,
        }).ToList();
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
    {
        var name = dto.RoleName.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new AppException("Role name cannot be empty.");

        if (name.Length > 50)
            throw new AppException("Role name must be 50 characters or fewer.");

        var existing = await _roleRepository.GetByNameAsync(name);
        if (existing is not null)
            throw new AppException($"A role named '{name}' already exists.");

        var role = new Role { RoleName = name };
        await _roleRepository.AddAsync(role);
        await _roleRepository.SaveChangesAsync();

        return new RoleDto { Id = role.Id, RoleName = role.RoleName, UserCount = 0 };
    }
}
