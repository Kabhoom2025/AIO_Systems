using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ILeaveRequestService
{
    Task<List<LeaveRequestDto>> GetAllAsync(int orgId);
    Task<LeaveRequestDto> GetByIdAsync(int orgId, int id);
    Task<LeaveRequestDto> CreateAsync(int orgId, CreateLeaveRequestDto dto);
    Task<LeaveRequestDto> UpdateAsync(int orgId, int id, UpdateLeaveRequestDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<LeaveRequestDto> ApproveAsync(int orgId, int id);
    Task<LeaveRequestDto> RejectAsync(int orgId, int id);
    Task<LeaveRequestDto> CancelAsync(int orgId, int id);
}
