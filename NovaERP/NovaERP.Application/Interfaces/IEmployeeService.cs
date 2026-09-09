using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync(int orgId);
    Task<EmployeeDto> GetByIdAsync(int orgId, int id);
    Task<EmployeeDto> CreateAsync(int orgId, CreateEmployeeDto dto);
    Task<EmployeeDto> UpdateAsync(int orgId, int id, UpdateEmployeeDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<EmployeeDto> TerminateAsync(int orgId, int id);
}
