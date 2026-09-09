using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class WorkflowInstanceRepository : IWorkflowInstanceRepository
{
    private readonly NovaErpDbContext _ctx;

    public WorkflowInstanceRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<WorkflowInstance> WithIncludes() =>
        _ctx.WorkflowInstances
            .Include(w => w.WorkflowDefinition)
            .Include(w => w.Steps).ThenInclude(s => s.ApproverRole)
            .Include(w => w.Steps).ThenInclude(s => s.ApproverUser);

    public Task<WorkflowInstance?> GetByIdAsync(int id) =>
        WithIncludes().FirstOrDefaultAsync(w => w.Id == id);

    public Task<List<WorkflowInstance>> GetPendingForRoleAsync(int orgId, int roleId) =>
        WithIncludes()
            .Where(w => w.WorkflowDefinition.OrganizationId == orgId && w.Status == "Pending" &&
                        w.Steps.Any(s => s.StepOrder == w.CurrentStepOrder && s.Status == "Pending" && s.ApproverRoleId == roleId))
            .OrderBy(w => w.SubmittedDate)
            .ToListAsync();

    public Task<List<WorkflowInstance>> GetByEntityAsync(int orgId, string entityType, int entityId) =>
        WithIncludes()
            .Where(w => w.WorkflowDefinition.OrganizationId == orgId && w.EntityType == entityType && w.EntityId == entityId)
            .OrderByDescending(w => w.SubmittedDate)
            .ToListAsync();

    public void Add(WorkflowInstance instance)    => _ctx.WorkflowInstances.Add(instance);
    public void Update(WorkflowInstance instance) => _ctx.WorkflowInstances.Update(instance);
    public Task SaveChangesAsync()                => _ctx.SaveChangesAsync();
}
