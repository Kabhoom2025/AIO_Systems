using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IEmployeePortalService
{
    Task<EmployeeDto> GetMyProfileAsync(int orgId, int userId);
    Task<List<LeaveRequestDto>> GetMyLeaveRequestsAsync(int orgId, int userId);
    Task<LeaveRequestDto> CreateMyLeaveRequestAsync(int orgId, int userId, CreateMyLeaveRequestDto dto);
    Task<LeaveRequestDto> CancelMyLeaveRequestAsync(int orgId, int userId, int leaveRequestId);
    Task<List<MyPayslipDto>> GetMyPayslipsAsync(int orgId, int userId);

    /// <summary>Leave types are non-sensitive reference data (unlike Employee/Compensation
    /// personal data) — passed through with no hrms.view check so the portal's own
    /// "Request Leave" dialog has a type picker without needing an admin permission.</summary>
    Task<List<LeaveTypeDto>> GetLeaveTypesAsync(int orgId);
}
