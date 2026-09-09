using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly NovaErpDbContext _ctx;

    public LeaveTypeRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<LeaveType>> GetAllByOrgAsync(int orgId) =>
        _ctx.LeaveTypes.Where(t => t.OrganizationId == orgId).OrderBy(t => t.Code).ToListAsync();

    public Task<LeaveType?> GetByIdAsync(int orgId, int id) =>
        _ctx.LeaveTypes.FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.LeaveTypes.AnyAsync(t => t.OrganizationId == orgId && t.Code == code);

    public void Add(LeaveType leaveType)    => _ctx.LeaveTypes.Add(leaveType);
    public void Update(LeaveType leaveType) => _ctx.LeaveTypes.Update(leaveType);
    public void Remove(LeaveType leaveType) => _ctx.LeaveTypes.Remove(leaveType);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
