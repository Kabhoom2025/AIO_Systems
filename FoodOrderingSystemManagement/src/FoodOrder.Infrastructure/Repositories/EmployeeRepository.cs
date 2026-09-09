using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;
    public EmployeeRepository(AppDbContext db) => _db = db;

    public Task SaveChangesAsync() => _db.SaveChangesAsync();

    private static DateTime Utc(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);

    // ── Attendance ────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<AttendanceRecord>> GetAttendanceByDateAsync(DateTime date)
        => await _db.AttendanceRecords
            .Include(a => a.User)
            .Where(a => a.Date == Utc(date))
            .OrderBy(a => a.User.Name)
            .ToListAsync();

    public async Task<IReadOnlyList<AttendanceRecord>> GetAttendanceByUserAsync(int userId, int month, int year)
        => await _db.AttendanceRecords
            .Include(a => a.User)
            .Where(a => a.UserId == userId && a.Date.Month == month && a.Date.Year == year)
            .OrderBy(a => a.Date)
            .ToListAsync();

    public Task<AttendanceRecord?> GetAttendanceAsync(int userId, DateTime date)
        => _db.AttendanceRecords
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == Utc(date));

    public async Task AddAttendanceAsync(AttendanceRecord record)
    {
        _db.AttendanceRecords.Add(record);
        await Task.CompletedTask;
    }

    // ── Salary ────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<EmployeeSalary>> GetAllSalaryConfigsAsync()
        => await _db.EmployeeSalaries.Include(s => s.User).ToListAsync();

    public Task<EmployeeSalary?> GetSalaryConfigAsync(int userId)
        => _db.EmployeeSalaries.FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task AddSalaryConfigAsync(EmployeeSalary config)
    {
        _db.EmployeeSalaries.Add(config);
        await Task.CompletedTask;
    }

    public async Task<IReadOnlyList<SalaryPayment>> GetPaymentsAsync(int? userId = null)
    {
        var q = _db.SalaryPayments.Include(p => p.User).AsQueryable();
        if (userId.HasValue) q = q.Where(p => p.UserId == userId.Value);
        return await q.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToListAsync();
    }

    public async Task AddPaymentAsync(SalaryPayment payment)
    {
        _db.SalaryPayments.Add(payment);
        await Task.CompletedTask;
    }

    public Task<SalaryPayment?> GetPaymentByIdAsync(int id)
        => _db.SalaryPayments.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);

    // ── Shifts ────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ShiftDefinition>> GetShiftDefinitionsAsync()
        => await _db.ShiftDefinitions.OrderBy(s => s.StartTime).ToListAsync();

    public Task<ShiftDefinition?> GetShiftDefinitionByIdAsync(int id)
        => _db.ShiftDefinitions.FindAsync(id).AsTask();

    public async Task AddShiftDefinitionAsync(ShiftDefinition shift)
    {
        _db.ShiftDefinitions.Add(shift);
        await Task.CompletedTask;
    }

    public async Task DeleteShiftDefinitionAsync(ShiftDefinition shift)
    {
        _db.ShiftDefinitions.Remove(shift);
        await Task.CompletedTask;
    }

    public async Task<IReadOnlyList<ShiftAssignment>> GetShiftAssignmentsAsync(DateTime start, DateTime end)
        => await _db.ShiftAssignments
            .Include(a => a.User)
            .Include(a => a.Shift)
            .Where(a => a.Date >= Utc(start) && a.Date <= Utc(end))
            .OrderBy(a => a.Date).ThenBy(a => a.Shift.StartTime)
            .ToListAsync();

    public Task<ShiftAssignment?> GetShiftAssignmentByIdAsync(int id)
        => _db.ShiftAssignments.FindAsync(id).AsTask();

    public async Task AddShiftAssignmentAsync(ShiftAssignment assignment)
    {
        _db.ShiftAssignments.Add(assignment);
        await Task.CompletedTask;
    }

    public async Task DeleteShiftAssignmentAsync(ShiftAssignment assignment)
    {
        _db.ShiftAssignments.Remove(assignment);
        await Task.CompletedTask;
    }

    // ── Performance ───────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PerformanceReview>> GetReviewsAsync(int? userId = null)
    {
        var q = _db.PerformanceReviews
            .Include(r => r.User)
            .Include(r => r.ReviewedBy)
            .AsQueryable();
        if (userId.HasValue) q = q.Where(r => r.UserId == userId.Value);
        return await q.OrderByDescending(r => r.ReviewDate).ToListAsync();
    }

    public Task<PerformanceReview?> GetReviewByIdAsync(int id)
        => _db.PerformanceReviews.FindAsync(id).AsTask();

    public async Task AddReviewAsync(PerformanceReview review)
    {
        _db.PerformanceReviews.Add(review);
        await Task.CompletedTask;
    }

    public async Task DeleteReviewAsync(PerformanceReview review)
    {
        _db.PerformanceReviews.Remove(review);
        await Task.CompletedTask;
    }
}
