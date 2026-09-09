using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _repo;

    public AttendanceService(IAttendanceRepository repo) => _repo = repo;

    public async Task<AttendanceRecordDto> CheckInAsync(int orgId, int employeeId, CheckInDto dto)
    {
        var employee = await _repo.GetEmployeeWithShiftAsync(orgId, employeeId)
            ?? throw new KeyNotFoundException("Employee not found.");

        var orgTimeZone = ResolveTimeZone(employee.Organization.Timezone);
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, orgTimeZone);
        var today = DateOnly.FromDateTime(localNow);

        var record = await _repo.GetByEmployeeAndDateAsync(orgId, employeeId, today);

        if (record != null && record.CheckInAt != null)
            throw new InvalidOperationException("Already checked in today.");

        var now = DateTime.UtcNow;
        var lateMinutes = 0;
        if (employee.Shift != null)
        {
            var graceEndLocal = today.ToDateTime(employee.Shift.StartTime).AddMinutes(employee.Shift.GraceMinutes);
            if (localNow > graceEndLocal)
                lateMinutes = (int)(localNow - graceEndLocal).TotalMinutes;
        }

        var source = string.IsNullOrWhiteSpace(dto.Source) ? "Web" : dto.Source;

        if (record == null)
        {
            record = new AttendanceRecord
            {
                OrganizationId = orgId,
                EmployeeId = employeeId,
                Date = today,
                CheckInAt = now,
                Status = "Present",
                LateMinutes = lateMinutes,
                Source = source,
                Location = dto.Location
            };
            _repo.AddRecord(record);
        }
        else
        {
            record.CheckInAt = now;
            record.Status = "Present";
            record.LateMinutes = lateMinutes;
            record.Source = source;
            record.Location = dto.Location;
            record.UpdatedDate = now;
            _repo.UpdateRecord(record);
        }

        await _repo.SaveChangesAsync();
        return MapToDto(record);
    }

    public async Task<AttendanceRecordDto> CheckOutAsync(int orgId, int employeeId)
    {
        var employee = await _repo.GetEmployeeWithShiftAsync(orgId, employeeId)
            ?? throw new KeyNotFoundException("Employee not found.");

        var orgTimeZone = ResolveTimeZone(employee.Organization.Timezone);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, orgTimeZone));
        var record = await _repo.GetByEmployeeAndDateAsync(orgId, employeeId, today);

        if (record == null || record.CheckInAt == null)
            throw new InvalidOperationException("Must check in before checking out.");

        var now = DateTime.UtcNow;
        record.CheckOutAt = now;
        var workedMinutes = (int)(now - record.CheckInAt.Value).TotalMinutes;
        record.WorkedMinutes = workedMinutes;

        if (employee.Shift != null)
        {
            var shift = employee.Shift;
            var start = shift.StartTime.ToTimeSpan();
            var end = shift.EndTime.ToTimeSpan();
            if (end < start) end = end.Add(TimeSpan.FromHours(24));

            var shiftDurationMinutes = (int)(end - start).TotalMinutes - shift.BreakMinutes;

            record.OvertimeMinutes = Math.Max(0, workedMinutes - shiftDurationMinutes);
            record.EarlyExitMinutes = Math.Max(0, shiftDurationMinutes - workedMinutes);
            record.Status = workedMinutes < shiftDurationMinutes / 2 ? "HalfDay" : "Present";
        }
        else
        {
            record.OvertimeMinutes = 0;
            record.EarlyExitMinutes = 0;
        }

        record.UpdatedDate = now;
        _repo.UpdateRecord(record);
        await _repo.SaveChangesAsync();
        return MapToDto(record);
    }

    public async Task<AttendanceRecordDto?> GetTodayAsync(int orgId, int employeeId)
    {
        var employee = await _repo.GetEmployeeWithShiftAsync(orgId, employeeId);
        var orgTimeZone = ResolveTimeZone(employee?.Organization.Timezone);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, orgTimeZone));
        var record = await _repo.GetByEmployeeAndDateAsync(orgId, employeeId, today);
        return record == null ? null : MapToDto(record);
    }

    private static TimeZoneInfo ResolveTimeZone(string? ianaId)
    {
        if (string.IsNullOrWhiteSpace(ianaId)) return TimeZoneInfo.Utc;
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    public async Task<List<AttendanceDayDto>> GetDailyAsync(int orgId, DateOnly date)
    {
        var records = await _repo.GetByDateAsync(orgId, date);
        return records.Select(r => new AttendanceDayDto
        {
            EmployeeId      = r.EmployeeId,
            EmployeeCode    = r.Employee.EmployeeCode,
            EmployeeName    = r.Employee.FullName,
            DepartmentName  = r.Employee.Department?.Name ?? string.Empty,
            CheckInAt       = r.CheckInAt,
            CheckOutAt      = r.CheckOutAt,
            Status          = r.Status,
            WorkedMinutes   = r.WorkedMinutes,
            LateMinutes     = r.LateMinutes,
            OvertimeMinutes = r.OvertimeMinutes
        }).ToList();
    }

    public async Task<List<AttendanceRecordDto>> GetByEmployeeMonthAsync(int orgId, int employeeId, int year, int month)
    {
        var records = await _repo.GetByEmployeeMonthAsync(orgId, employeeId, year, month);
        return records.Select(MapToDto).ToList();
    }

    public async Task<AttendanceSummaryDto> GetSummaryAsync(int orgId, DateOnly date)
    {
        var totalActive = await _repo.GetActiveEmployeeCountAsync(orgId);
        var records = await _repo.GetByDateAsync(orgId, date);

        var present = records.Count(r => r.Status == "Present" || r.Status == "HalfDay");
        var onLeave = records.Count(r => r.Status == "Leave");
        var late = records.Count(r => r.LateMinutes > 0);
        var absent = Math.Max(0, totalActive - records.Count);

        return new AttendanceSummaryDto
        {
            TotalEmployees = totalActive,
            Present        = present,
            Absent         = absent,
            OnLeave        = onLeave,
            Late           = late
        };
    }

    public async Task<RegularizationDto> CreateRegularizationAsync(int orgId, int employeeId, CreateRegularizationDto dto)
    {
        var regularization = new AttendanceRegularization
        {
            OrganizationId    = orgId,
            EmployeeId        = employeeId,
            Date              = dto.Date,
            RequestedCheckIn  = dto.RequestedCheckIn,
            RequestedCheckOut = dto.RequestedCheckOut,
            Reason            = dto.Reason,
            Status            = "Pending"
        };
        _repo.AddRegularization(regularization);
        await _repo.SaveChangesAsync();

        var saved = await _repo.GetRegularizationByIdAsync(orgId, regularization.Id);
        return MapToDto(saved!);
    }

    public async Task<List<RegularizationDto>> GetRegularizationsAsync(int orgId)
    {
        var regularizations = await _repo.GetRegularizationsForOrgAsync(orgId);
        return regularizations.Select(MapToDto).ToList();
    }

    public async Task<List<RegularizationDto>> GetMyRegularizationsAsync(int orgId, int employeeId)
    {
        var regularizations = await _repo.GetRegularizationsByEmployeeAsync(orgId, employeeId);
        return regularizations.Select(MapToDto).ToList();
    }

    public async Task<RegularizationDto> ApproveRegularizationAsync(int orgId, int id, int reviewerUserId, ReviewDto dto)
    {
        var regularization = await _repo.GetRegularizationByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Regularization {id} not found.");

        if (regularization.Status != "Pending")
            throw new InvalidOperationException("Regularization has already been reviewed.");

        regularization.Status           = "Approved";
        regularization.ReviewedByUserId = reviewerUserId;
        regularization.ReviewedAt       = DateTime.UtcNow;
        regularization.ReviewNotes      = dto.Notes;
        _repo.UpdateRegularization(regularization);

        var record = await _repo.GetByEmployeeAndDateAsync(orgId, regularization.EmployeeId, regularization.Date);
        if (record == null)
        {
            record = new AttendanceRecord
            {
                OrganizationId = orgId,
                EmployeeId     = regularization.EmployeeId,
                Date           = regularization.Date,
                Source         = "Web"
            };
            _repo.AddRecord(record);
        }

        if (regularization.RequestedCheckIn.HasValue)
            record.CheckInAt = regularization.RequestedCheckIn;
        if (regularization.RequestedCheckOut.HasValue)
            record.CheckOutAt = regularization.RequestedCheckOut;

        if (record.CheckInAt.HasValue && record.CheckOutAt.HasValue)
            record.WorkedMinutes = (int)(record.CheckOutAt.Value - record.CheckInAt.Value).TotalMinutes;

        record.Status = "Present";
        record.UpdatedDate = DateTime.UtcNow;
        _repo.UpdateRecord(record);

        await _repo.SaveChangesAsync();
        return MapToDto(regularization);
    }

    public async Task<RegularizationDto> RejectRegularizationAsync(int orgId, int id, int reviewerUserId, ReviewDto dto)
    {
        var regularization = await _repo.GetRegularizationByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Regularization {id} not found.");

        if (regularization.Status != "Pending")
            throw new InvalidOperationException("Regularization has already been reviewed.");

        regularization.Status           = "Rejected";
        regularization.ReviewedByUserId = reviewerUserId;
        regularization.ReviewedAt       = DateTime.UtcNow;
        regularization.ReviewNotes      = dto.Notes;
        _repo.UpdateRegularization(regularization);
        await _repo.SaveChangesAsync();
        return MapToDto(regularization);
    }

    private static AttendanceRecordDto MapToDto(AttendanceRecord r) => new()
    {
        Id               = r.Id,
        EmployeeId       = r.EmployeeId,
        Date             = r.Date,
        CheckInAt        = r.CheckInAt,
        CheckOutAt       = r.CheckOutAt,
        Status           = r.Status,
        WorkedMinutes    = r.WorkedMinutes,
        OvertimeMinutes  = r.OvertimeMinutes,
        LateMinutes      = r.LateMinutes,
        EarlyExitMinutes = r.EarlyExitMinutes,
        Source           = r.Source,
        Location         = r.Location,
        Notes            = r.Notes
    };

    private static RegularizationDto MapToDto(AttendanceRegularization r) => new()
    {
        Id                = r.Id,
        EmployeeId        = r.EmployeeId,
        EmployeeName      = r.Employee?.FullName ?? string.Empty,
        Date              = r.Date,
        RequestedCheckIn  = r.RequestedCheckIn,
        RequestedCheckOut = r.RequestedCheckOut,
        Reason            = r.Reason,
        Status            = r.Status,
        ReviewedByUserId  = r.ReviewedByUserId,
        ReviewedAt        = r.ReviewedAt,
        ReviewNotes       = r.ReviewNotes,
        CreatedDate       = r.CreatedDate
    };
}
