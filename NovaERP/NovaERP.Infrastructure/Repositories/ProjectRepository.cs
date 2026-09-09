using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly NovaErpDbContext _ctx;

    public ProjectRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<Project> Query() =>
        _ctx.Projects.Include(p => p.Manager);

    public Task<List<Project>> GetAllByOrgAsync(int orgId) =>
        Query().Where(p => p.OrganizationId == orgId).OrderBy(p => p.Code).ToListAsync();

    public Task<Project?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.Projects.AnyAsync(p => p.OrganizationId == orgId && p.Code == code);

    public async Task<Dictionary<int, (int Total, int Completed)>> GetTaskCountsByOrgAsync(int orgId)
    {
        var rows = await _ctx.ProjectTasks
            .Where(t => t.OrganizationId == orgId)
            .GroupBy(t => t.ProjectId)
            .Select(g => new { ProjectId = g.Key, Total = g.Count(), Completed = g.Count(t => t.Status == "Done") })
            .ToListAsync();

        return rows.ToDictionary(r => r.ProjectId, r => (r.Total, r.Completed));
    }

    public async Task<(int Total, int Completed)> GetTaskCountsAsync(int orgId, int projectId)
    {
        var total = await _ctx.ProjectTasks.CountAsync(t => t.OrganizationId == orgId && t.ProjectId == projectId);
        var completed = await _ctx.ProjectTasks.CountAsync(t => t.OrganizationId == orgId && t.ProjectId == projectId && t.Status == "Done");
        return (total, completed);
    }

    public Task<bool> HasTasksAsync(int orgId, int projectId) =>
        _ctx.ProjectTasks.AnyAsync(t => t.OrganizationId == orgId && t.ProjectId == projectId);

    public void Add(Project project)    => _ctx.Projects.Add(project);
    public void Update(Project project) => _ctx.Projects.Update(project);
    public void Remove(Project project) => _ctx.Projects.Remove(project);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
