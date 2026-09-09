using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class EmployeeCompensationRepository : IEmployeeCompensationRepository
{
    private readonly NovaErpDbContext _ctx;

    public EmployeeCompensationRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<EmployeeCompensation> Query() =>
        _ctx.EmployeeCompensations.Include(c => c.Employee);

    public Task<List<EmployeeCompensation>> GetAllByOrgAsync(int orgId) =>
        Query().Where(c => c.OrganizationId == orgId).OrderBy(c => c.Employee.EmployeeCode).ToListAsync();

    public Task<EmployeeCompensation?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId);

    public Task<EmployeeCompensation?> GetByEmployeeIdAsync(int orgId, int employeeId) =>
        Query().FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.OrganizationId == orgId);

    public Task<bool> EmployeeIdExistsAsync(int orgId, int employeeId) =>
        _ctx.EmployeeCompensations.AnyAsync(c => c.OrganizationId == orgId && c.EmployeeId == employeeId);

    public void Add(EmployeeCompensation compensation)    => _ctx.EmployeeCompensations.Add(compensation);
    public void Update(EmployeeCompensation compensation) => _ctx.EmployeeCompensations.Update(compensation);
    public void Remove(EmployeeCompensation compensation) => _ctx.EmployeeCompensations.Remove(compensation);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
