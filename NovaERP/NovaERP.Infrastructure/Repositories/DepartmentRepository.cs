using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly NovaErpDbContext _ctx;

    public DepartmentRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Department>> GetAllByOrgAsync(int orgId) =>
        _ctx.Departments
            .Include(d => d.Branch)
            .Include(d => d.Parent)
            .Where(d => d.OrganizationId == orgId)
            .OrderBy(d => d.Name)
            .ToListAsync();

    public Task<Department?> GetByIdAsync(int orgId, int id) =>
        _ctx.Departments
            .Include(d => d.Branch)
            .Include(d => d.Parent)
            .Include(d => d.Children)
            .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == orgId);

    public void Add(Department department)    => _ctx.Departments.Add(department);
    public void Update(Department department) => _ctx.Departments.Update(department);
    public void Remove(Department department) => _ctx.Departments.Remove(department);
    public Task SaveChangesAsync()             => _ctx.SaveChangesAsync();
}
