using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync(int orgId);
    Task<RoleDto?> GetByIdAsync(int orgId, int id);
    Task<RoleDto> CreateAsync(int orgId, CreateRoleDto dto);
    Task<RoleDto> UpdateAsync(int orgId, int id, UpdateRoleDto dto);
    Task DeleteAsync(int orgId, int id);
}
