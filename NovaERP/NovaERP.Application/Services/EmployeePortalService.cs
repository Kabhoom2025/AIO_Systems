using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

/// <summary>Self-service orchestration for a logged-in User's own Employee record — no
/// hrms.view/payroll.view permission is checked anywhere here (see the module's design note
/// in PROJECT_PLAN.md): those stay Admin-only for the admin pages, but an ordinary Employee-
/// role user has neither permission, so this service scopes every query to "mine" via
/// Employee.UserId == the caller's UserId instead of gating by a permission.</summary>
public class EmployeePortalService : IEmployeePortalService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveRequestRepository _leaveRequestRepo;
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly IPayRunRepository _payRunRepo;
    private readonly ILeaveTypeService _leaveTypeService;
    private readonly IValidator<CreateMyLeaveRequestDto> _createLeaveValidator;

    public EmployeePortalService(IEmployeeRepository employeeRepo, IEmployeeService employeeService,
        ILeaveRequestRepository leaveRequestRepo, ILeaveRequestService leaveRequestService,
        IPayRunRepository payRunRepo, ILeaveTypeService leaveTypeService,
        IValidator<CreateMyLeaveRequestDto> createLeaveValidator)
    {
        _employeeRepo = employeeRepo;
        _employeeService = employeeService;
        _leaveRequestRepo = leaveRequestRepo;
        _leaveRequestService = leaveRequestService;
        _payRunRepo = payRunRepo;
        _leaveTypeService = leaveTypeService;
        _createLeaveValidator = createLeaveValidator;
    }

    public Task<List<LeaveTypeDto>> GetLeaveTypesAsync(int orgId) => _leaveTypeService.GetAllAsync(orgId);

    private async Task<Employee> GetMyEmployeeAsync(int orgId, int userId) =>
        await _employeeRepo.GetByUserIdAsync(orgId, userId)
            ?? throw new KeyNotFoundException("No employee record is linked to your account.");

    public async Task<EmployeeDto> GetMyProfileAsync(int orgId, int userId)
    {
        var employee = await GetMyEmployeeAsync(orgId, userId);
        return await _employeeService.GetByIdAsync(orgId, employee.Id);
    }

    public async Task<List<LeaveRequestDto>> GetMyLeaveRequestsAsync(int orgId, int userId)
    {
        var employee = await GetMyEmployeeAsync(orgId, userId);
        var requests = await _leaveRequestRepo.GetAllByEmployeeIdAsync(orgId, employee.Id);
        return requests.Select(ToDto).ToList();
    }

    public async Task<LeaveRequestDto> CreateMyLeaveRequestAsync(int orgId, int userId, CreateMyLeaveRequestDto dto)
    {
        await _createLeaveValidator.ValidateAndThrowAsync(dto);

        var employee = await GetMyEmployeeAsync(orgId, userId);
        return await _leaveRequestService.CreateAsync(orgId, new CreateLeaveRequestDto
        {
            EmployeeId = employee.Id,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            DaysRequested = dto.DaysRequested,
            Reason = dto.Reason
        });
    }

    public async Task<LeaveRequestDto> CancelMyLeaveRequestAsync(int orgId, int userId, int leaveRequestId)
    {
        var employee = await GetMyEmployeeAsync(orgId, userId);

        var request = await _leaveRequestRepo.GetByIdAsync(orgId, leaveRequestId)
            ?? throw new KeyNotFoundException($"LeaveRequest {leaveRequestId} not found");

        if (request.EmployeeId != employee.Id)
            throw new UnauthorizedAccessException("You can only cancel your own leave requests.");

        return await _leaveRequestService.CancelAsync(orgId, leaveRequestId);
    }

    public async Task<List<MyPayslipDto>> GetMyPayslipsAsync(int orgId, int userId)
    {
        var employee = await GetMyEmployeeAsync(orgId, userId);
        var lines = await _payRunRepo.GetPaidLinesByEmployeeIdAsync(orgId, employee.Id);

        return lines.Select(l => new MyPayslipDto
        {
            PayRunId = l.PayRunId,
            RunNumber = l.PayRun?.RunNumber ?? string.Empty,
            PeriodMonth = l.PayRun?.PeriodMonth ?? 0,
            PeriodYear = l.PayRun?.PeriodYear ?? 0,
            BasicSalary = l.BasicSalary,
            Hra = l.Hra,
            OtherAllowances = l.OtherAllowances,
            Deductions = l.Deductions,
            GrossPay = l.GrossPay,
            NetPay = l.NetPay
        }).ToList();
    }

    private static LeaveRequestDto ToDto(LeaveRequest l) => new()
    {
        Id = l.Id,
        EmployeeId = l.EmployeeId,
        EmployeeName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}" : string.Empty,
        LeaveTypeId = l.LeaveTypeId,
        LeaveTypeName = l.LeaveType?.Name ?? string.Empty,
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        DaysRequested = l.DaysRequested,
        Reason = l.Reason,
        Status = l.Status
    };
}
