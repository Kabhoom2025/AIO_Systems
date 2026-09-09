using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository _repo;
    private readonly IWorkflowEngine _workflowEngine;

    public LeaveService(ILeaveRepository repo, IWorkflowEngine workflowEngine)
    {
        _repo = repo;
        _workflowEngine = workflowEngine;
    }

    public async Task<List<LeaveTypeDto>> GetTypesAsync(int orgId)
    {
        var types = await _repo.GetAllTypesByOrgAsync(orgId);
        return types.Select(MapToDto).ToList();
    }

    public async Task<LeaveTypeDto> CreateTypeAsync(int orgId, CreateLeaveTypeDto dto)
    {
        var type = new LeaveType
        {
            OrganizationId   = orgId,
            Name             = dto.Name,
            Code             = dto.Code,
            IsPaid           = dto.IsPaid,
            AnnualQuota      = dto.AnnualQuota,
            MaxCarryForward  = dto.MaxCarryForward,
            RequiresApproval = dto.RequiresApproval,
            Color            = dto.Color,
            IsActive         = true
        };
        _repo.AddType(type);
        await _repo.SaveChangesAsync();
        return MapToDto(type);
    }

    public async Task<LeaveTypeDto> UpdateTypeAsync(int orgId, int id, UpdateLeaveTypeDto dto)
    {
        var type = await _repo.GetTypeByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Leave type {id} not found.");

        type.Name             = dto.Name;
        type.Code             = dto.Code;
        type.IsPaid           = dto.IsPaid;
        type.AnnualQuota      = dto.AnnualQuota;
        type.MaxCarryForward  = dto.MaxCarryForward;
        type.RequiresApproval = dto.RequiresApproval;
        type.Color            = dto.Color;
        type.IsActive         = dto.IsActive;
        type.UpdatedDate      = DateTime.UtcNow;

        _repo.UpdateType(type);
        await _repo.SaveChangesAsync();
        return MapToDto(type);
    }

    public async Task DeleteTypeAsync(int orgId, int id)
    {
        var type = await _repo.GetTypeByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Leave type {id} not found.");

        if (await _repo.TypeHasRequestsOrBalancesAsync(id))
            throw new InvalidOperationException("Cannot delete a leave type with existing requests or balances.");

        _repo.RemoveType(type);
        await _repo.SaveChangesAsync();
    }

    public async Task<List<LeaveBalanceDto>> GetMyBalancesAsync(int orgId, int employeeId)
    {
        var balances = await _repo.GetBalancesByEmployeeYearAsync(orgId, employeeId, DateTime.UtcNow.Year);
        return balances.Select(MapToDto).ToList();
    }

    public async Task<List<LeaveBalanceDto>> GetBalancesForEmployeeAsync(int orgId, int employeeId)
    {
        var balances = await _repo.GetBalancesByEmployeeYearAsync(orgId, employeeId, DateTime.UtcNow.Year);
        return balances.Select(MapToDto).ToList();
    }

    public async Task<int> AllocateBalancesAsync(int orgId, AllocateBalancesDto dto)
    {
        var employees = await _repo.GetActiveEmployeesAsync(orgId);
        var types = await _repo.GetActiveTypesAsync(orgId);
        var existing = await _repo.GetBalancesByYearAsync(orgId, dto.Year);
        var existingKeys = existing.Select(b => (b.EmployeeId, b.LeaveTypeId)).ToHashSet();

        var created = 0;
        foreach (var employee in employees)
        {
            foreach (var type in types)
            {
                if (existingKeys.Contains((employee.Id, type.Id)))
                    continue;

                _repo.AddBalance(new LeaveBalance
                {
                    OrganizationId = orgId,
                    EmployeeId     = employee.Id,
                    LeaveTypeId    = type.Id,
                    Year           = dto.Year,
                    Allocated      = type.AnnualQuota,
                    Used           = 0,
                    CarriedForward = 0
                });
                created++;
            }
        }

        if (created > 0)
            await _repo.SaveChangesAsync();

        return created;
    }

    public async Task<LeaveRequestDto> CreateRequestAsync(int orgId, int employeeId, CreateLeaveRequestDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            throw new InvalidOperationException("End date must not be before start date.");

        var type = await _repo.GetTypeByIdAsync(orgId, dto.LeaveTypeId)
            ?? throw new KeyNotFoundException($"Leave type {dto.LeaveTypeId} not found.");

        var days = dto.IsHalfDay ? 0.5m : dto.EndDate.DayNumber - dto.StartDate.DayNumber + 1;

        if (type.RequiresApproval)
        {
            var balance = await _repo.GetBalanceAsync(orgId, employeeId, dto.LeaveTypeId, dto.StartDate.Year);
            if (balance == null || balance.Available < days)
                throw new InvalidOperationException("Insufficient leave balance.");
        }

        var request = new LeaveRequest
        {
            OrganizationId = orgId,
            EmployeeId     = employeeId,
            LeaveTypeId    = dto.LeaveTypeId,
            StartDate      = dto.StartDate,
            EndDate        = dto.EndDate,
            Days           = days,
            IsHalfDay      = dto.IsHalfDay,
            Reason         = dto.Reason,
            Status         = "Pending"
        };
        _repo.AddRequest(request);
        await _repo.SaveChangesAsync();

        var saved = await _repo.GetRequestByIdAsync(orgId, request.Id);
        await RunSubmissionWorkflowAsync(orgId, saved!);

        // An AutoApprove/AutoReject action (if the workflow took that path) already changed the
        // status — re-fetch so the caller sees the outcome, not the stale "Pending" snapshot.
        var final = await _repo.GetRequestByIdAsync(orgId, request.Id);
        return MapToDto(final!);
    }

    private async Task RunSubmissionWorkflowAsync(int orgId, LeaveRequest request)
    {
        var employeeUserId = await _repo.GetUserIdByEmployeeIdAsync(request.EmployeeId);
        var managerUserId  = await _repo.GetUserIdByEmployeeIdAsync(request.Employee.ManagerId);

        var context = new Dictionary<string, object?>
        {
            ["Days"]             = request.Days,
            ["IsHalfDay"]        = request.IsHalfDay,
            ["IsPaid"]           = request.LeaveType.IsPaid,
            ["LeaveTypeName"]    = request.LeaveType.Name,
            ["DepartmentName"]   = request.Employee.Department?.Name,
            ["DesignationTitle"] = request.Employee.Designation?.Title,
            ["EmploymentType"]   = request.Employee.EmploymentType,
            ["EmployeeName"]     = request.Employee.FullName,
            ["EmployeeUserId"]   = employeeUserId,
            ["ManagerUserId"]    = managerUserId
        };

        var actionHandlers = new Dictionary<string, Func<Dictionary<string, string>, Task>>
        {
            ["AutoApprove"] = _ => AutoApproveAsync(orgId, request.Id),
            ["AutoReject"]  = _ => AutoRejectAsync(orgId, request.Id)
        };

        await _workflowEngine.TriggerAsync(orgId, "LeaveRequestSubmitted", "LeaveRequest", request.Id, context, actionHandlers);
    }

    private async Task AutoApproveAsync(int orgId, int requestId)
    {
        var request = await _repo.GetRequestByIdAsync(orgId, requestId);
        if (request == null || request.Status != "Pending") return;
        await ApproveCoreAsync(orgId, request, reviewerUserId: null, notes: "Auto-approved by workflow.");
    }

    private async Task AutoRejectAsync(int orgId, int requestId)
    {
        var request = await _repo.GetRequestByIdAsync(orgId, requestId);
        if (request == null || request.Status != "Pending") return;

        request.Status           = "Rejected";
        request.ReviewedByUserId = null;
        request.ReviewedAt       = DateTime.UtcNow;
        request.ReviewNotes      = "Auto-rejected by workflow.";
        _repo.UpdateRequest(request);
        await _repo.SaveChangesAsync();
    }

    public async Task<List<LeaveRequestDto>> GetMyRequestsAsync(int orgId, int employeeId)
    {
        var requests = await _repo.GetRequestsByEmployeeAsync(orgId, employeeId);
        return requests.Select(MapToDto).ToList();
    }

    public async Task<List<LeaveRequestDto>> GetRequestsAsync(int orgId, string? status)
    {
        var requests = await _repo.GetRequestsForOrgAsync(orgId, status);
        return requests.Select(MapToDto).ToList();
    }

    public async Task<List<LeaveRequestDto>> GetPendingRequestsAsync(int orgId)
    {
        var requests = await _repo.GetPendingRequestsAsync(orgId);
        return requests.Select(MapToDto).ToList();
    }

    public async Task<LeaveRequestDto> ApproveRequestAsync(int orgId, int id, int reviewerUserId, ReviewDto dto)
    {
        var request = await _repo.GetRequestByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Leave request {id} not found.");

        if (request.Status != "Pending")
            throw new InvalidOperationException("Leave request has already been reviewed.");

        await ApproveCoreAsync(orgId, request, reviewerUserId, dto.Notes);
        return MapToDto(request);
    }

    private async Task ApproveCoreAsync(int orgId, LeaveRequest request, int? reviewerUserId, string? notes)
    {
        request.Status           = "Approved";
        request.ReviewedByUserId = reviewerUserId;
        request.ReviewedAt       = DateTime.UtcNow;
        request.ReviewNotes      = notes;
        _repo.UpdateRequest(request);

        var balance = await _repo.GetBalanceAsync(orgId, request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);
        if (balance == null)
        {
            balance = new LeaveBalance
            {
                OrganizationId = orgId,
                EmployeeId     = request.EmployeeId,
                LeaveTypeId    = request.LeaveTypeId,
                Year           = request.StartDate.Year,
                Allocated      = request.LeaveType.AnnualQuota,
                Used           = 0,
                CarriedForward = 0
            };
            _repo.AddBalance(balance);
        }
        balance.Used += request.Days;
        balance.UpdatedDate = DateTime.UtcNow;
        _repo.UpdateBalance(balance);

        await _repo.SaveChangesAsync();
    }

    public async Task<LeaveRequestDto> RejectRequestAsync(int orgId, int id, int reviewerUserId, ReviewDto dto)
    {
        var request = await _repo.GetRequestByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Leave request {id} not found.");

        if (request.Status != "Pending")
            throw new InvalidOperationException("Leave request has already been reviewed.");

        request.Status           = "Rejected";
        request.ReviewedByUserId = reviewerUserId;
        request.ReviewedAt       = DateTime.UtcNow;
        request.ReviewNotes      = dto.Notes;
        _repo.UpdateRequest(request);
        await _repo.SaveChangesAsync();
        return MapToDto(request);
    }

    public async Task<LeaveRequestDto> CancelRequestAsync(int orgId, int employeeId, int id)
    {
        var request = await _repo.GetRequestByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Leave request {id} not found.");

        if (request.EmployeeId != employeeId)
            throw new InvalidOperationException("You can only cancel your own leave request.");

        if (request.Status == "Approved")
        {
            var balance = await _repo.GetBalanceAsync(orgId, request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);
            if (balance != null)
            {
                balance.Used = Math.Max(0, balance.Used - request.Days);
                balance.UpdatedDate = DateTime.UtcNow;
                _repo.UpdateBalance(balance);
            }
        }
        else if (request.Status != "Pending")
        {
            throw new InvalidOperationException("Only pending or approved leave requests can be cancelled.");
        }

        request.Status = "Cancelled";
        request.UpdatedDate = DateTime.UtcNow;
        _repo.UpdateRequest(request);

        await _repo.SaveChangesAsync();
        return MapToDto(request);
    }

    public async Task<List<LeaveCalendarDto>> GetCalendarAsync(int orgId, DateOnly from, DateOnly to)
    {
        var requests = await _repo.GetApprovedRequestsInRangeAsync(orgId, from, to);
        return requests.Select(r => new LeaveCalendarDto
        {
            EmployeeName  = r.Employee.FullName,
            LeaveTypeName = r.LeaveType.Name,
            Color         = r.LeaveType.Color,
            StartDate     = r.StartDate,
            EndDate       = r.EndDate
        }).ToList();
    }

    private static LeaveTypeDto MapToDto(LeaveType t) => new()
    {
        Id               = t.Id,
        Name             = t.Name,
        Code             = t.Code,
        IsPaid           = t.IsPaid,
        AnnualQuota      = t.AnnualQuota,
        MaxCarryForward  = t.MaxCarryForward,
        RequiresApproval = t.RequiresApproval,
        Color            = t.Color,
        IsActive         = t.IsActive
    };

    private static LeaveBalanceDto MapToDto(LeaveBalance b) => new()
    {
        Id             = b.Id,
        EmployeeId     = b.EmployeeId,
        LeaveTypeId    = b.LeaveTypeId,
        LeaveTypeName  = b.LeaveType?.Name ?? string.Empty,
        Year           = b.Year,
        Allocated      = b.Allocated,
        Used           = b.Used,
        CarriedForward = b.CarriedForward,
        Available      = b.Available
    };

    private static LeaveRequestDto MapToDto(LeaveRequest r) => new()
    {
        Id               = r.Id,
        EmployeeId       = r.EmployeeId,
        EmployeeName     = r.Employee?.FullName ?? string.Empty,
        LeaveTypeId      = r.LeaveTypeId,
        LeaveTypeName    = r.LeaveType?.Name ?? string.Empty,
        StartDate        = r.StartDate,
        EndDate          = r.EndDate,
        Days             = r.Days,
        IsHalfDay        = r.IsHalfDay,
        Reason           = r.Reason,
        Status           = r.Status,
        ReviewedByUserId = r.ReviewedByUserId,
        ReviewedAt       = r.ReviewedAt,
        ReviewNotes      = r.ReviewNotes,
        CreatedDate      = r.CreatedDate
    };
}
