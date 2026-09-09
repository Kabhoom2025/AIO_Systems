using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class DesignationRepository : IDesignationRepository
{
    private readonly HrmsDbContext _ctx;

    public DesignationRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<Designation>> GetAllByOrgAsync(int orgId) =>
        _ctx.Designations
            .Include(d => d.JobGrade)
            .Where(d => d.OrganizationId == orgId)
            .OrderBy(d => d.Title)
            .ToListAsync();

    public Task<Designation?> GetByIdAsync(int orgId, int id) =>
        _ctx.Designations
            .Include(d => d.JobGrade)
            .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == orgId);

    public void Add(Designation designation)    => _ctx.Designations.Add(designation);
    public void Update(Designation designation) => _ctx.Designations.Update(designation);
    public void Remove(Designation designation) => _ctx.Designations.Remove(designation);

    public Task<List<JobGrade>> GetJobGradesByOrgAsync(int orgId) =>
        _ctx.JobGrades
            .Where(g => g.OrganizationId == orgId)
            .OrderBy(g => g.Level)
            .ToListAsync();

    public Task<JobGrade?> GetJobGradeByIdAsync(int orgId, int id) =>
        _ctx.JobGrades.FirstOrDefaultAsync(g => g.Id == id && g.OrganizationId == orgId);

    public Task<bool> IsJobGradeInUseAsync(int jobGradeId) =>
        _ctx.Designations.AnyAsync(d => d.JobGradeId == jobGradeId);

    public void AddJobGrade(JobGrade jobGrade)    => _ctx.JobGrades.Add(jobGrade);
    public void UpdateJobGrade(JobGrade jobGrade) => _ctx.JobGrades.Update(jobGrade);
    public void RemoveJobGrade(JobGrade jobGrade) => _ctx.JobGrades.Remove(jobGrade);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
