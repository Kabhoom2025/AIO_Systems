using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateRoleDto> _createValidator;
    private readonly IValidator<UpdateRoleDto> _updateValidator;

    public RoleService(IRoleRepository repo, IMapper mapper,
        IValidator<CreateRoleDto> createValidator, IValidator<UpdateRoleDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<RoleDto>> GetAllAsync(int orgId)
    {
        var roles = await _repo.GetAllForOrgAsync(orgId);
        return roles.Select(_mapper.Map<RoleDto>).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(int orgId, int id)
    {
        var role = await _repo.GetByIdAsync(id);
        if (role == null || (role.OrganizationId != null && role.OrganizationId != orgId))
            return null;
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> CreateAsync(int orgId, CreateRoleDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var permissions = await ResolvePermissionsAsync(dto.PermissionKeys);

        var role = new Role
        {
            OrganizationId  = orgId,
            Name            = dto.Name,
            Description     = dto.Description,
            IsSystemRole    = false,
            RolePermissions = permissions
                .Select(p => new RolePermission { PermissionId = p.Id, Permission = p })
                .ToList()
        };
        _repo.Add(role);
        await _repo.SaveChangesAsync();
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> UpdateAsync(int orgId, int id, UpdateRoleDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var role = await GetOwnedRoleAsync(orgId, id);

        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be modified.");

        var permissions = await ResolvePermissionsAsync(dto.PermissionKeys);

        role.Name        = dto.Name;
        role.Description = dto.Description;
        role.UpdatedDate = DateTime.UtcNow;

        role.RolePermissions.Clear();
        foreach (var permission in permissions)
            role.RolePermissions.Add(new RolePermission
            {
                RoleId       = role.Id,
                PermissionId = permission.Id,
                Permission   = permission
            });

        _repo.Update(role);
        await _repo.SaveChangesAsync();
        return _mapper.Map<RoleDto>(role);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var role = await GetOwnedRoleAsync(orgId, id);

        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be deleted.");
        if (role.Users.Count > 0)
            throw new InvalidOperationException("Cannot delete a role that is assigned to users.");

        _repo.Remove(role);
        await _repo.SaveChangesAsync();
    }

    private async Task<Role> GetOwnedRoleAsync(int orgId, int id)
    {
        var role = await _repo.GetByIdAsync(id);
        if (role == null || (role.OrganizationId != null && role.OrganizationId != orgId))
            throw new KeyNotFoundException($"Role {id} not found");
        return role;
    }

    private async Task<List<Permission>> ResolvePermissionsAsync(List<string> keys)
    {
        var distinctKeys = keys.Distinct().ToList();
        var permissions  = await _repo.GetPermissionsByKeysAsync(distinctKeys);

        if (permissions.Count != distinctKeys.Count)
        {
            var missing = distinctKeys.Except(permissions.Select(p => p.Key)).ToList();
            throw new InvalidOperationException($"Unknown permission keys: {string.Join(", ", missing)}");
        }

        return permissions;
    }
}
