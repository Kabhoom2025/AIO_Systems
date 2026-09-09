using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ProjectTaskRepository : IProjectTaskRepository
{
    private readonly NovaErpDbContext _ctx;

    public ProjectTaskRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<ProjectTask> Query() =>
        _ctx.ProjectTasks.Include(t => t.Project).Include(t => t.AssignedTo);

    public Task<List<ProjectTask>> GetAllByOrgAsync(int orgId) =>
        Query().Where(t => t.OrganizationId == orgId).OrderBy(t => t.DueDate).ToListAsync();

    public Task<ProjectTask?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == orgId);

    public void Add(ProjectTask task)    => _ctx.ProjectTasks.Add(task);
    public void Update(ProjectTask task) => _ctx.ProjectTasks.Update(task);
    public void Remove(ProjectTask task) => _ctx.ProjectTasks.Remove(task);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
