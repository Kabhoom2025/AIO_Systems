using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly NovaErpDbContext _ctx;

    public EmployeeRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<Employee> Query() =>
        _ctx.Employees
            .Include(e => e.Department)
            .Include(e => e.User)
            .Include(e => e.ReportingManager);

    public Task<List<Employee>> GetAllByOrgAsync(int orgId) =>
        Query().Where(e => e.OrganizationId == orgId).OrderBy(e => e.EmployeeCode).ToListAsync();

    public Task<Employee?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == orgId);

    public Task<Employee?> GetByUserIdAsync(int orgId, int userId) =>
        Query().FirstOrDefaultAsync(e => e.UserId == userId && e.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.Employees.AnyAsync(e => e.OrganizationId == orgId && e.EmployeeCode == code);

    public void Add(Employee employee)    => _ctx.Employees.Add(employee);
    public void Update(Employee employee) => _ctx.Employees.Update(employee);
    public void Remove(Employee employee) => _ctx.Employees.Remove(employee);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
