using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync(int orgId);
    Task<DepartmentDto> GetByIdAsync(int orgId, int id);
    Task<DepartmentDto> CreateAsync(int orgId, CreateDepartmentDto dto);
    Task<DepartmentDto> UpdateAsync(int orgId, int id, UpdateDepartmentDto dto);
    Task DeleteAsync(int orgId, int id);
}
