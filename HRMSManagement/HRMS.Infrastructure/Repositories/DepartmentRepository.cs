using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly HrmsDbContext _ctx;

    public DepartmentRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<Department>> GetAllByOrgAsync(int orgId) =>
        _ctx.Departments
            .Include(d => d.Branch)
            .Include(d => d.Parent)
            .Include(d => d.HeadEmployee)
            .Where(d => d.OrganizationId == orgId)
            .OrderBy(d => d.Name)
            .ToListAsync();

    public Task<Department?> GetByIdAsync(int orgId, int id) =>
        _ctx.Departments
            .Include(d => d.Branch)
            .Include(d => d.Parent)
            .Include(d => d.HeadEmployee)
            .Include(d => d.Children)
            .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == orgId);

    public Task<Dictionary<int, int>> GetEmployeeCountsAsync(int orgId) =>
        _ctx.Employees
            .Where(e => e.OrganizationId == orgId)
            .GroupBy(e => e.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);

    public Task<int> GetEmployeeCountAsync(int departmentId) =>
        _ctx.Employees.CountAsync(e => e.DepartmentId == departmentId);

    public void Add(Department department)    => _ctx.Departments.Add(department);
    public void Update(Department department) => _ctx.Departments.Update(department);
    public void Remove(Department department) => _ctx.Departments.Remove(department);
    public Task SaveChangesAsync()             => _ctx.SaveChangesAsync();
}
