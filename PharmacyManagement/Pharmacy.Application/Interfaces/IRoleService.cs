using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync(int orgId);
    Task<RoleDto?> GetByIdAsync(int id);
    Task<RoleDto> CreateAsync(int orgId, CreateRoleDto dto);
    Task<RoleDto> UpdateAsync(int id, UpdateRoleDto dto);
    Task DeleteAsync(int id);
}
