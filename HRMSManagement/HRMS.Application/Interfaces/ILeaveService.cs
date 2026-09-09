using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface ILeaveService
{
    // Leave types
    Task<List<LeaveTypeDto>> GetTypesAsync(int orgId);
    Task<LeaveTypeDto> CreateTypeAsync(int orgId, CreateLeaveTypeDto dto);
    Task<LeaveTypeDto> UpdateTypeAsync(int orgId, int id, UpdateLeaveTypeDto dto);
    Task DeleteTypeAsync(int orgId, int id);

    // Balances
    Task<List<LeaveBalanceDto>> GetMyBalancesAsync(int orgId, int employeeId);
    Task<List<LeaveBalanceDto>> GetBalancesForEmployeeAsync(int orgId, int employeeId);
    Task<int> AllocateBalancesAsync(int orgId, AllocateBalancesDto dto);

    // Requests
    Task<LeaveRequestDto> CreateRequestAsync(int orgId, int employeeId, CreateLeaveRequestDto dto);
    Task<List<LeaveRequestDto>> GetMyRequestsAsync(int orgId, int employeeId);
    Task<List<LeaveRequestDto>> GetRequestsAsync(int orgId, string? status);
    Task<List<LeaveRequestDto>> GetPendingRequestsAsync(int orgId);
    Task<LeaveRequestDto> ApproveRequestAsync(int orgId, int id, int reviewerUserId, ReviewDto dto);
    Task<LeaveRequestDto> RejectRequestAsync(int orgId, int id, int reviewerUserId, ReviewDto dto);
    Task<LeaveRequestDto> CancelRequestAsync(int orgId, int employeeId, int id);
    Task<List<LeaveCalendarDto>> GetCalendarAsync(int orgId, DateOnly from, DateOnly to);
}
