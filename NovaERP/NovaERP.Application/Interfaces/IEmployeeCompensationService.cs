using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IEmployeeCompensationService
{
    Task<List<EmployeeCompensationDto>> GetAllAsync(int orgId);
    Task<EmployeeCompensationDto> GetByIdAsync(int orgId, int id);
    Task<EmployeeCompensationDto> CreateAsync(int orgId, CreateEmployeeCompensationDto dto);
    Task<EmployeeCompensationDto> UpdateAsync(int orgId, int id, UpdateEmployeeCompensationDto dto);
    Task DeleteAsync(int orgId, int id);
}
