using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ILeaveTypeRepository _leaveTypeRepo;
    private readonly IValidator<CreateLeaveRequestDto> _createValidator;
    private readonly IValidator<UpdateLeaveRequestDto> _updateValidator;

    public LeaveRequestService(ILeaveRequestRepository repo, IEmployeeRepository employeeRepo,
        ILeaveTypeRepository leaveTypeRepo,
        IValidator<CreateLeaveRequestDto> createValidator, IValidator<UpdateLeaveRequestDto> updateValidator)
    {
        _repo = repo;
        _employeeRepo = employeeRepo;
        _leaveTypeRepo = leaveTypeRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<LeaveRequestDto>> GetAllAsync(int orgId)
    {
        var requests = await _repo.GetAllByOrgAsync(orgId);
        return requests.Select(ToDto).ToList();
    }

    public async Task<LeaveRequestDto> GetByIdAsync(int orgId, int id)
    {
        var request = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LeaveRequest {id} not found");
        return ToDto(request);
    }

    public async Task<LeaveRequestDto> CreateAsync(int orgId, CreateLeaveRequestDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        _ = await _employeeRepo.GetByIdAsync(orgId, dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee {dto.EmployeeId} not found");
        _ = await _leaveTypeRepo.GetByIdAsync(orgId, dto.LeaveTypeId)
            ?? throw new KeyNotFoundException($"LeaveType {dto.LeaveTypeId} not found");

        var request = new LeaveRequest
        {
            OrganizationId = orgId,
            EmployeeId = dto.EmployeeId,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            DaysRequested = dto.DaysRequested,
            Reason = dto.Reason,
            Status = "Pending"
        };

        _repo.Add(request);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, request.Id) ?? request;
        return ToDto(reloaded);
    }

    public async Task<LeaveRequestDto> UpdateAsync(int orgId, int id, UpdateLeaveRequestDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var request = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LeaveRequest {id} not found");

        if (request.Status != "Pending")
            throw new InvalidOperationException("Only pending leave requests can be edited.");

        _ = await _leaveTypeRepo.GetByIdAsync(orgId, dto.LeaveTypeId)
            ?? throw new KeyNotFoundException($"LeaveType {dto.LeaveTypeId} not found");

        request.LeaveTypeId = dto.LeaveTypeId;
        request.StartDate = dto.StartDate;
        request.EndDate = dto.EndDate;
        request.DaysRequested = dto.DaysRequested;
        request.Reason = dto.Reason;
        request.UpdatedDate = DateTime.UtcNow;

        _repo.Update(request);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, request.Id) ?? request;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var request = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LeaveRequest {id} not found");

        if (request.Status != "Pending")
            throw new InvalidOperationException("Only pending leave requests can be deleted.");

        _repo.Remove(request);
        await _repo.SaveChangesAsync();
    }

    public async Task<LeaveRequestDto> ApproveAsync(int orgId, int id) =>
        await TransitionAsync(orgId, id, "Approved");

    public async Task<LeaveRequestDto> RejectAsync(int orgId, int id) =>
        await TransitionAsync(orgId, id, "Rejected");

    public async Task<LeaveRequestDto> CancelAsync(int orgId, int id) =>
        await TransitionAsync(orgId, id, "Cancelled");

    private async Task<LeaveRequestDto> TransitionAsync(int orgId, int id, string newStatus)
    {
        var request = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LeaveRequest {id} not found");

        if (request.Status != "Pending")
            throw new InvalidOperationException($"Only pending leave requests can be {newStatus.ToLowerInvariant()}.");

        request.Status = newStatus;
        request.UpdatedDate = DateTime.UtcNow;
        _repo.Update(request);
        await _repo.SaveChangesAsync();

        return ToDto(request);
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
