using FoodOrder.Application.DTOs.Employee;
using FoodOrder.Application.DTOs.User;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUserRepository     _userRepo;
    private readonly IEmployeeRepository _empRepo;
    private readonly ILedgerService      _ledgerService;

    public EmployeeService(IUserRepository userRepo, IEmployeeRepository empRepo, ILedgerService ledgerService)
    {
        _userRepo      = userRepo;
        _empRepo       = empRepo;
        _ledgerService = ledgerService;
    }

    // ── User edit / delete ───────────────────────────────────────────────────

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdWithRoleAsync(id);
        if (user is null) return null;

        if (!user.Email.Equals(dto.Email.Trim().ToLower(), StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userRepo.GetByEmailAsync(dto.Email.Trim().ToLower());
            if (existing is not null)
                throw new AppException($"Email '{dto.Email}' is already in use.");
        }

        user.Name   = dto.Name.Trim();
        user.Email  = dto.Email.Trim().ToLower();
        user.RoleId = dto.RoleId;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        var updated = await _userRepo.GetByIdWithRoleAsync(user.Id);
        return MapUserToDto(updated!);
    }

    public async Task DeleteUserAsync(int id, int requestingUserId)
    {
        if (id == requestingUserId)
            throw new AppException("You cannot delete your own account.");

        var user = await _userRepo.GetByIdWithRoleAsync(id)
            ?? throw new NotFoundException("User", id);

        _userRepo.Delete(user);
        await _userRepo.SaveChangesAsync();
    }

    // ── Attendance ───────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<AttendanceDto>> GetAttendanceByDateAsync(DateTime date)
    {
        var records = await _empRepo.GetAttendanceByDateAsync(date);
        return records.Select(MapAttendance).ToList();
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetAttendanceByUserAsync(int userId, int month, int year)
    {
        var records = await _empRepo.GetAttendanceByUserAsync(userId, month, year);
        return records.Select(MapAttendance).ToList();
    }

    public async Task<AttendanceDto> UpsertAttendanceAsync(UpsertAttendanceRequest req)
    {
        var record = await _empRepo.GetAttendanceAsync(req.UserId, req.Date);
        if (record is null)
        {
            record = new AttendanceRecord
            {
                UserId      = req.UserId,
                Date        = DateTime.SpecifyKind(req.Date.Date, DateTimeKind.Utc),
                CreatedDate = DateTime.UtcNow,
            };
            await _empRepo.AddAttendanceAsync(record);
        }

        record.Status   = req.Status;
        record.Notes    = req.Notes?.Trim();
        record.CheckIn  = ParseTime(req.CheckIn);
        record.CheckOut = ParseTime(req.CheckOut);

        await _empRepo.SaveChangesAsync();

        var user = await _userRepo.GetByIdWithRoleAsync(req.UserId);
        record.User = user!;
        return MapAttendance(record);
    }

    // ── Salary ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<EmployeeSalaryDto>> GetSalaryConfigsAsync(int? organizationId)
    {
        var users   = await _userRepo.GetAllWithRolesAsync(organizationId);
        var configs = await _empRepo.GetAllSalaryConfigsAsync();
        var configMap = configs.ToDictionary(c => c.UserId);

        return users.Select(u =>
        {
            configMap.TryGetValue(u.Id, out var cfg);
            return new EmployeeSalaryDto
            {
                Id          = cfg?.Id ?? 0,
                UserId      = u.Id,
                UserName    = u.Name,
                BasicSalary = cfg?.BasicSalary ?? 0,
                Allowances  = cfg?.Allowances ?? 0,
                PayPeriod   = cfg?.PayPeriod ?? "Monthly",
            };
        }).ToList();
    }

    public async Task<EmployeeSalaryDto?> UpdateSalaryConfigAsync(int userId, UpdateSalaryConfigRequest req)
    {
        var user = await _userRepo.GetByIdWithRoleAsync(userId);
        if (user is null) return null;

        var cfg = await _empRepo.GetSalaryConfigAsync(userId);
        if (cfg is null)
        {
            cfg = new EmployeeSalary { UserId = userId, CreatedDate = DateTime.UtcNow };
            await _empRepo.AddSalaryConfigAsync(cfg);
        }

        cfg.BasicSalary = req.BasicSalary;
        cfg.Allowances  = req.Allowances;
        cfg.PayPeriod   = req.PayPeriod;
        await _empRepo.SaveChangesAsync();

        return new EmployeeSalaryDto
        {
            Id = cfg.Id, UserId = userId, UserName = user.Name,
            BasicSalary = cfg.BasicSalary, Allowances = cfg.Allowances, PayPeriod = cfg.PayPeriod,
        };
    }

    public async Task<IReadOnlyList<SalaryPaymentDto>> GetPaymentsAsync(int? userId = null)
    {
        var list = await _empRepo.GetPaymentsAsync(userId);
        return list.Select(MapPayment).ToList();
    }

    public async Task<SalaryPaymentDto> CreatePaymentAsync(CreateSalaryPaymentRequest req)
    {
        var payment = new SalaryPayment
        {
            UserId      = req.UserId,
            Month       = req.Month,
            Year        = req.Year,
            GrossPay    = req.GrossPay,
            Deductions  = req.Deductions,
            NetPay      = req.GrossPay - req.Deductions,
            Status      = "Pending",
            Notes       = req.Notes?.Trim(),
            CreatedDate = DateTime.UtcNow,
        };
        await _empRepo.AddPaymentAsync(payment);
        await _empRepo.SaveChangesAsync();

        var user = await _userRepo.GetByIdWithRoleAsync(req.UserId);
        payment.User = user!;
        return MapPayment(payment);
    }

    public async Task<SalaryPaymentDto?> MarkPaymentPaidAsync(int paymentId, int paidById)
    {
        var payment = await _empRepo.GetPaymentByIdAsync(paymentId);
        if (payment is null) return null;

        payment.Status   = "Paid";
        payment.PaidDate = DateTime.UtcNow;
        await _empRepo.SaveChangesAsync();

        var monthName = new DateTime(payment.Year, payment.Month, 1).ToString("MMMM yyyy");
        await _ledgerService.CreateAsync(new DTOs.CreateLedgerEntryRequest
        {
            Date     = DateTime.UtcNow,
            Type     = "Debit",
            Category = "Salary",
            Amount   = payment.NetPay,
            Note     = $"Salary paid – {payment.User?.Name ?? "Employee"} ({monthName})",
        }, paidById);

        return MapPayment(payment);
    }

    // ── Shifts ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ShiftDefinitionDto>> GetShiftDefinitionsAsync()
    {
        var list = await _empRepo.GetShiftDefinitionsAsync();
        return list.Select(MapShiftDef).ToList();
    }

    public async Task<ShiftDefinitionDto> CreateShiftDefinitionAsync(CreateShiftDefinitionRequest req)
    {
        var shift = new ShiftDefinition
        {
            Name        = req.Name.Trim(),
            StartTime   = ParseTimeSpan(req.StartTime),
            EndTime     = ParseTimeSpan(req.EndTime),
            ColorCode   = req.ColorCode ?? "#1976d2",
            CreatedDate = DateTime.UtcNow,
        };
        await _empRepo.AddShiftDefinitionAsync(shift);
        await _empRepo.SaveChangesAsync();
        return MapShiftDef(shift);
    }

    public async Task DeleteShiftDefinitionAsync(int id)
    {
        var shift = await _empRepo.GetShiftDefinitionByIdAsync(id);
        if (shift is null) return;
        await _empRepo.DeleteShiftDefinitionAsync(shift);
        await _empRepo.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ShiftAssignmentDto>> GetShiftAssignmentsAsync(DateTime startDate, DateTime endDate)
    {
        var list = await _empRepo.GetShiftAssignmentsAsync(startDate, endDate);
        return list.Select(MapAssignment).ToList();
    }

    public async Task<ShiftAssignmentDto> CreateShiftAssignmentAsync(CreateShiftAssignmentRequest req)
    {
        var shift    = await _empRepo.GetShiftDefinitionByIdAsync(req.ShiftId);
        var user     = await _userRepo.GetByIdWithRoleAsync(req.UserId);
        var assignment = new ShiftAssignment
        {
            UserId      = req.UserId,
            ShiftId     = req.ShiftId,
            Date        = DateTime.SpecifyKind(req.Date.Date, DateTimeKind.Utc),
            CreatedDate = DateTime.UtcNow,
            User        = user!,
            Shift       = shift!,
        };
        await _empRepo.AddShiftAssignmentAsync(assignment);
        await _empRepo.SaveChangesAsync();
        return MapAssignment(assignment);
    }

    public async Task DeleteShiftAssignmentAsync(int id)
    {
        var a = await _empRepo.GetShiftAssignmentByIdAsync(id);
        if (a is null) return;
        await _empRepo.DeleteShiftAssignmentAsync(a);
        await _empRepo.SaveChangesAsync();
    }

    // ── Performance ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PerformanceReviewDto>> GetReviewsAsync(int? userId = null)
    {
        var list = await _empRepo.GetReviewsAsync(userId);
        return list.Select(MapReview).ToList();
    }

    public async Task<PerformanceReviewDto> CreateReviewAsync(int reviewedById, CreatePerformanceReviewRequest req)
    {
        var reviewer = await _userRepo.GetByIdWithRoleAsync(reviewedById);
        var employee = await _userRepo.GetByIdWithRoleAsync(req.UserId);

        var review = new PerformanceReview
        {
            UserId       = req.UserId,
            ReviewedById = reviewedById,
            ReviewDate   = DateTime.SpecifyKind(req.ReviewDate.Date, DateTimeKind.Utc),
            Rating       = req.Rating,
            Category     = req.Category,
            Comments     = req.Comments.Trim(),
            CreatedDate  = DateTime.UtcNow,
            User         = employee!,
            ReviewedBy   = reviewer!,
        };
        await _empRepo.AddReviewAsync(review);
        await _empRepo.SaveChangesAsync();
        return MapReview(review);
    }

    public async Task DeleteReviewAsync(int id)
    {
        var r = await _empRepo.GetReviewByIdAsync(id);
        if (r is null) return;
        await _empRepo.DeleteReviewAsync(r);
        await _empRepo.SaveChangesAsync();
    }

    // ── Static mappers ───────────────────────────────────────────────────────

    private static UserDto MapUserToDto(User u) => new()
    {
        Id = u.Id, Name = u.Name, Email = u.Email,
        RoleId = u.RoleId, RoleName = u.Role?.RoleName ?? string.Empty,
        IsActive = u.IsActive, CreatedDate = u.CreatedDate,
    };

    private static AttendanceDto MapAttendance(AttendanceRecord a) => new()
    {
        Id = a.Id, UserId = a.UserId, UserName = a.User?.Name ?? string.Empty,
        Date = a.Date,
        CheckIn  = a.CheckIn.HasValue  ? $"{a.CheckIn.Value.Hours:D2}:{a.CheckIn.Value.Minutes:D2}"  : null,
        CheckOut = a.CheckOut.HasValue ? $"{a.CheckOut.Value.Hours:D2}:{a.CheckOut.Value.Minutes:D2}" : null,
        Status = a.Status, Notes = a.Notes,
    };

    private static SalaryPaymentDto MapPayment(SalaryPayment p) => new()
    {
        Id = p.Id, UserId = p.UserId, UserName = p.User?.Name ?? string.Empty,
        Month = p.Month, Year = p.Year, GrossPay = p.GrossPay,
        Deductions = p.Deductions, NetPay = p.NetPay,
        Status = p.Status, PaidDate = p.PaidDate, Notes = p.Notes,
    };

    private static ShiftDefinitionDto MapShiftDef(ShiftDefinition s) => new()
    {
        Id = s.Id, Name = s.Name,
        StartTime = FormatTime(s.StartTime),
        EndTime   = FormatTime(s.EndTime),
        ColorCode = s.ColorCode,
    };

    private static ShiftAssignmentDto MapAssignment(ShiftAssignment a) => new()
    {
        Id = a.Id, UserId = a.UserId, UserName = a.User?.Name ?? string.Empty,
        ShiftId = a.ShiftId, ShiftName = a.Shift?.Name ?? string.Empty,
        ShiftStart = a.Shift != null ? FormatTime(a.Shift.StartTime) : string.Empty,
        ShiftEnd   = a.Shift != null ? FormatTime(a.Shift.EndTime)   : string.Empty,
        ShiftColor = a.Shift?.ColorCode, Date = a.Date,
    };

    private static PerformanceReviewDto MapReview(PerformanceReview r) => new()
    {
        Id = r.Id, UserId = r.UserId, UserName = r.User?.Name ?? string.Empty,
        ReviewedById = r.ReviewedById, ReviewedByName = r.ReviewedBy?.Name ?? string.Empty,
        ReviewDate = r.ReviewDate, Rating = r.Rating, Category = r.Category, Comments = r.Comments,
    };

    private static string FormatTime(TimeSpan t) => $"{t.Hours:D2}:{t.Minutes:D2}";

    private static TimeSpan? ParseTime(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var parts = s.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[0], out var h) && int.TryParse(parts[1], out var m))
            return new TimeSpan(h, m, 0);
        return null;
    }

    private static TimeSpan ParseTimeSpan(string s) => ParseTime(s) ?? TimeSpan.Zero;
}
