using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repo;

    public RoleService(IRoleRepository repo) => _repo = repo;

    public async Task<List<RoleDto>> GetAllAsync(int orgId)
    {
        var roles = await _repo.GetAllAsync(orgId);
        return roles.Select(MapToDto).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var role = await _repo.GetByIdAsync(id);
        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateAsync(int orgId, CreateRoleDto dto)
    {
        var permissions = await _repo.GetPermissionsByKeysAsync(dto.PermissionKeys);
        var role = new Role
        {
            OrganizationId = orgId,
            Name           = dto.Name,
            IsSystemRole   = false,
            RolePermissions = permissions.Select(p => new RolePermission { Permission = p }).ToList()
        };
        _repo.Add(role);
        await _repo.SaveChangesAsync();
        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateAsync(int id, UpdateRoleDto dto)
    {
        var role = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Role {id} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be modified.");

        var permissions = await _repo.GetPermissionsByKeysAsync(dto.PermissionKeys);
        role.Name = dto.Name;
        role.RolePermissions.Clear();
        foreach (var p in permissions)
            role.RolePermissions.Add(new RolePermission { Role = role, Permission = p });

        role.UpdatedDate = DateTime.UtcNow;
        await _repo.SaveChangesAsync();
        return MapToDto(role);
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Role {id} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be deleted.");

        _repo.Remove(role);
        await _repo.SaveChangesAsync();
    }

    private static RoleDto MapToDto(Role r) => new()
    {
        Id             = r.Id,
        Name           = r.Name,
        IsSystemRole   = r.IsSystemRole,
        PermissionKeys = r.RolePermissions.Select(rp => rp.Permission.Key).ToList()
    };
}
