using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly HrmsDbContext _ctx;

    public EmployeeRepository(HrmsDbContext ctx) => _ctx = ctx;

    public async Task<(List<Employee> Items, int Total)> GetPagedAsync(
        int orgId, string? search, int? departmentId, int? branchId, string? status, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 20;

        var query = _ctx.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Manager)
            .Where(e => e.OrganizationId == orgId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(e =>
                e.EmployeeCode.ToLower().Contains(s) ||
                e.FirstName.ToLower().Contains(s) ||
                e.LastName.ToLower().Contains(s) ||
                e.WorkEmail.ToLower().Contains(s));
        }

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (branchId.HasValue)
            query = query.Where(e => e.BranchId == branchId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<List<Employee>> GetLookupAsync(int orgId) =>
        _ctx.Employees
            .Include(e => e.Designation)
            .Where(e => e.OrganizationId == orgId && e.Status == "Active")
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
            .ToListAsync();

    public Task<Employee?> GetByIdAsync(int orgId, int id) =>
        _ctx.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Shift)
            .Include(e => e.Manager)
            .Include(e => e.Documents)
            .Include(e => e.Educations)
            .Include(e => e.Experiences)
            .Include(e => e.FamilyMembers)
            .Include(e => e.LifecycleEvents)
            .FirstOrDefaultAsync(e => e.OrganizationId == orgId && e.Id == id);

    public Task<List<Employee>> GetDirectReportsAsync(int orgId, int managerId) =>
        _ctx.Employees
            .Include(e => e.Designation)
            .Where(e => e.OrganizationId == orgId && e.ManagerId == managerId)
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
            .ToListAsync();

    public Task<List<Employee>> GetAllForExportAsync(int orgId) =>
        _ctx.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Where(e => e.OrganizationId == orgId)
            .OrderBy(e => e.EmployeeCode)
            .ToListAsync();

    public async Task<int> GetNextCodeNumberAsync(int orgId)
    {
        var codes = await _ctx.Employees
            .Where(e => e.OrganizationId == orgId)
            .Select(e => e.EmployeeCode)
            .ToListAsync();

        var max = 0;
        foreach (var code in codes)
        {
            var digits = new string(code.Where(char.IsDigit).ToArray());
            if (digits.Length > 0 && int.TryParse(digits, out var n) && n > max)
                max = n;
        }
        return max + 1;
    }

    public Task<bool> WorkEmailExistsAsync(int orgId, string workEmail, int? excludeId)
    {
        var email = workEmail.Trim().ToLower();
        return _ctx.Employees.AnyAsync(e =>
            e.OrganizationId == orgId &&
            e.WorkEmail.ToLower() == email &&
            (!excludeId.HasValue || e.Id != excludeId.Value));
    }

    public void Add(Employee employee)    => _ctx.Employees.Add(employee);
    public void Update(Employee employee) => _ctx.Employees.Update(employee);

    public Task<EmployeeDocument?> GetDocumentAsync(int employeeId, int docId) =>
        _ctx.EmployeeDocuments.FirstOrDefaultAsync(d => d.EmployeeId == employeeId && d.Id == docId);

    public void AddDocument(EmployeeDocument document)    => _ctx.EmployeeDocuments.Add(document);
    public void RemoveDocument(EmployeeDocument document) => _ctx.EmployeeDocuments.Remove(document);

    public Task<EmployeeEducation?> GetEducationAsync(int employeeId, int eduId) =>
        _ctx.EmployeeEducations.FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Id == eduId);

    public void AddEducation(EmployeeEducation education)    => _ctx.EmployeeEducations.Add(education);
    public void RemoveEducation(EmployeeEducation education) => _ctx.EmployeeEducations.Remove(education);

    public Task<EmployeeExperience?> GetExperienceAsync(int employeeId, int expId) =>
        _ctx.EmployeeExperiences.FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Id == expId);

    public void AddExperience(EmployeeExperience experience)    => _ctx.EmployeeExperiences.Add(experience);
    public void RemoveExperience(EmployeeExperience experience) => _ctx.EmployeeExperiences.Remove(experience);

    public Task<EmployeeFamilyMember?> GetFamilyMemberAsync(int employeeId, int memberId) =>
        _ctx.EmployeeFamilyMembers.FirstOrDefaultAsync(f => f.EmployeeId == employeeId && f.Id == memberId);

    public void AddFamilyMember(EmployeeFamilyMember member)    => _ctx.EmployeeFamilyMembers.Add(member);
    public void RemoveFamilyMember(EmployeeFamilyMember member) => _ctx.EmployeeFamilyMembers.Remove(member);

    public Task<List<EmployeeLifecycleEvent>> GetLifecycleEventsAsync(int employeeId) =>
        _ctx.EmployeeLifecycleEvents
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.EventDate)
            .ThenByDescending(l => l.Id)
            .ToListAsync();

    public void AddLifecycleEvent(EmployeeLifecycleEvent lifecycleEvent) =>
        _ctx.EmployeeLifecycleEvents.Add(lifecycleEvent);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
