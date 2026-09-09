using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly HrmsDbContext _ctx;

    public AttendanceRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<Employee?> GetEmployeeWithShiftAsync(int orgId, int employeeId) =>
        _ctx.Employees
            .Include(e => e.Shift)
            .Include(e => e.Organization)
            .FirstOrDefaultAsync(e => e.OrganizationId == orgId && e.Id == employeeId);

    public Task<int> GetActiveEmployeeCountAsync(int orgId) =>
        _ctx.Employees.CountAsync(e => e.OrganizationId == orgId && e.Status == "Active");

    public Task<AttendanceRecord?> GetByEmployeeAndDateAsync(int orgId, int employeeId, DateOnly date) =>
        _ctx.AttendanceRecords
            .FirstOrDefaultAsync(a => a.OrganizationId == orgId && a.EmployeeId == employeeId && a.Date == date);

    public Task<List<AttendanceRecord>> GetByDateAsync(int orgId, DateOnly date) =>
        _ctx.AttendanceRecords
            .Include(a => a.Employee).ThenInclude(e => e.Department)
            .Where(a => a.OrganizationId == orgId && a.Date == date)
            .OrderBy(a => a.Employee.FirstName).ThenBy(a => a.Employee.LastName)
            .ToListAsync();

    public Task<List<AttendanceRecord>> GetByEmployeeMonthAsync(int orgId, int employeeId, int year, int month) =>
        _ctx.AttendanceRecords
            .Where(a => a.OrganizationId == orgId && a.EmployeeId == employeeId &&
                        a.Date.Year == year && a.Date.Month == month)
            .OrderBy(a => a.Date)
            .ToListAsync();

    public void AddRecord(AttendanceRecord record)    => _ctx.AttendanceRecords.Add(record);
    public void UpdateRecord(AttendanceRecord record) => _ctx.AttendanceRecords.Update(record);

    public Task<AttendanceRegularization?> GetRegularizationByIdAsync(int orgId, int id) =>
        _ctx.AttendanceRegularizations
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.OrganizationId == orgId && r.Id == id);

    public Task<List<AttendanceRegularization>> GetRegularizationsForOrgAsync(int orgId) =>
        _ctx.AttendanceRegularizations
            .Include(r => r.Employee)
            .Where(r => r.OrganizationId == orgId)
            .OrderBy(r => r.Status == "Pending" ? 0 : 1)
            .ThenByDescending(r => r.CreatedDate)
            .Take(200)
            .ToListAsync();

    public Task<List<AttendanceRegularization>> GetRegularizationsByEmployeeAsync(int orgId, int employeeId) =>
        _ctx.AttendanceRegularizations
            .Include(r => r.Employee)
            .Where(r => r.OrganizationId == orgId && r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

    public void AddRegularization(AttendanceRegularization regularization) =>
        _ctx.AttendanceRegularizations.Add(regularization);

    public void UpdateRegularization(AttendanceRegularization regularization) =>
        _ctx.AttendanceRegularizations.Update(regularization);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
