using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class LeaveRepository : ILeaveRepository
{
    private readonly HrmsDbContext _ctx;

    public LeaveRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<LeaveType>> GetAllTypesByOrgAsync(int orgId) =>
        _ctx.LeaveTypes.Where(t => t.OrganizationId == orgId).OrderBy(t => t.Name).ToListAsync();

    public Task<LeaveType?> GetTypeByIdAsync(int orgId, int id) =>
        _ctx.LeaveTypes.FirstOrDefaultAsync(t => t.OrganizationId == orgId && t.Id == id);

    public Task<List<LeaveType>> GetActiveTypesAsync(int orgId) =>
        _ctx.LeaveTypes.Where(t => t.OrganizationId == orgId && t.IsActive).ToListAsync();

    public async Task<bool> TypeHasRequestsOrBalancesAsync(int typeId) =>
        await _ctx.LeaveRequests.AnyAsync(r => r.LeaveTypeId == typeId) ||
        await _ctx.LeaveBalances.AnyAsync(b => b.LeaveTypeId == typeId);

    public void AddType(LeaveType type)    => _ctx.LeaveTypes.Add(type);
    public void UpdateType(LeaveType type) => _ctx.LeaveTypes.Update(type);
    public void RemoveType(LeaveType type) => _ctx.LeaveTypes.Remove(type);

    public Task<List<LeaveBalance>> GetBalancesByEmployeeYearAsync(int orgId, int employeeId, int year) =>
        _ctx.LeaveBalances
            .Include(b => b.LeaveType)
            .Where(b => b.OrganizationId == orgId && b.EmployeeId == employeeId && b.Year == year)
            .OrderBy(b => b.LeaveType.Name)
            .ToListAsync();

    public Task<LeaveBalance?> GetBalanceAsync(int orgId, int employeeId, int leaveTypeId, int year) =>
        _ctx.LeaveBalances.FirstOrDefaultAsync(b =>
            b.OrganizationId == orgId && b.EmployeeId == employeeId &&
            b.LeaveTypeId == leaveTypeId && b.Year == year);

    public Task<List<LeaveBalance>> GetBalancesByYearAsync(int orgId, int year) =>
        _ctx.LeaveBalances.Where(b => b.OrganizationId == orgId && b.Year == year).ToListAsync();

    public Task<List<Employee>> GetActiveEmployeesAsync(int orgId) =>
        _ctx.Employees.Where(e => e.OrganizationId == orgId && e.Status == "Active").ToListAsync();

    public void AddBalance(LeaveBalance balance)    => _ctx.LeaveBalances.Add(balance);
    public void UpdateBalance(LeaveBalance balance) => _ctx.LeaveBalances.Update(balance);

    public Task<LeaveRequest?> GetRequestByIdAsync(int orgId, int id) =>
        _ctx.LeaveRequests
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.Designation)
            .Include(r => r.LeaveType)
            .FirstOrDefaultAsync(r => r.OrganizationId == orgId && r.Id == id);

    public Task<int?> GetUserIdByEmployeeIdAsync(int? employeeId)
    {
        if (employeeId == null) return Task.FromResult((int?)null);
        return _ctx.Users
            .Where(u => u.EmployeeId == employeeId)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync();
    }

    public Task<List<LeaveRequest>> GetRequestsByEmployeeAsync(int orgId, int employeeId) =>
        _ctx.LeaveRequests
            .Include(r => r.LeaveType)
            .Where(r => r.OrganizationId == orgId && r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

    public Task<List<LeaveRequest>> GetRequestsForOrgAsync(int orgId, string? status)
    {
        var query = _ctx.LeaveRequests
            .Include(r => r.Employee)
            .Include(r => r.LeaveType)
            .Where(r => r.OrganizationId == orgId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        return query.OrderByDescending(r => r.CreatedDate).ToListAsync();
    }

    public Task<List<LeaveRequest>> GetPendingRequestsAsync(int orgId) =>
        _ctx.LeaveRequests
            .Include(r => r.Employee)
            .Include(r => r.LeaveType)
            .Where(r => r.OrganizationId == orgId && r.Status == "Pending")
            .OrderBy(r => r.StartDate)
            .ToListAsync();

    public Task<List<LeaveRequest>> GetApprovedRequestsInRangeAsync(int orgId, DateOnly from, DateOnly to) =>
        _ctx.LeaveRequests
            .Include(r => r.Employee)
            .Include(r => r.LeaveType)
            .Where(r => r.OrganizationId == orgId && r.Status == "Approved" &&
                        r.StartDate <= to && r.EndDate >= from)
            .OrderBy(r => r.StartDate)
            .ToListAsync();

    public void AddRequest(LeaveRequest request)    => _ctx.LeaveRequests.Add(request);
    public void UpdateRequest(LeaveRequest request) => _ctx.LeaveRequests.Update(request);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
