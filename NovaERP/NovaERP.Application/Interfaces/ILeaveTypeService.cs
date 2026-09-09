using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ILeaveTypeService
{
    Task<List<LeaveTypeDto>> GetAllAsync(int orgId);
    Task<LeaveTypeDto> GetByIdAsync(int orgId, int id);
    Task<LeaveTypeDto> CreateAsync(int orgId, CreateLeaveTypeDto dto);
    Task<LeaveTypeDto> UpdateAsync(int orgId, int id, UpdateLeaveTypeDto dto);
    Task DeleteAsync(int orgId, int id);
}
