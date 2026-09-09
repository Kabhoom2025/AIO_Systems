using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly NovaErpDbContext _ctx;

    public LeaveRequestRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<LeaveRequest> Query() =>
        _ctx.LeaveRequests.Include(l => l.Employee).Include(l => l.LeaveType);

    public Task<List<LeaveRequest>> GetAllByOrgAsync(int orgId) =>
        Query().Where(l => l.OrganizationId == orgId).OrderByDescending(l => l.StartDate).ToListAsync();

    public Task<List<LeaveRequest>> GetAllByEmployeeIdAsync(int orgId, int employeeId) =>
        Query().Where(l => l.OrganizationId == orgId && l.EmployeeId == employeeId)
            .OrderByDescending(l => l.StartDate).ToListAsync();

    public Task<LeaveRequest?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(l => l.Id == id && l.OrganizationId == orgId);

    public void Add(LeaveRequest leaveRequest)    => _ctx.LeaveRequests.Add(leaveRequest);
    public void Update(LeaveRequest leaveRequest) => _ctx.LeaveRequests.Update(leaveRequest);
    public void Remove(LeaveRequest leaveRequest) => _ctx.LeaveRequests.Remove(leaveRequest);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
