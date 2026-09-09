using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly NovaErpDbContext _ctx;

    public WorkflowDefinitionRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<WorkflowDefinition>> GetAllByOrgAsync(int orgId) =>
        _ctx.WorkflowDefinitions
            .Include(w => w.Steps).ThenInclude(s => s.ApproverRole)
            .Where(w => w.OrganizationId == orgId)
            .OrderBy(w => w.Name)
            .ToListAsync();

    public Task<WorkflowDefinition?> GetByIdAsync(int orgId, int id) =>
        _ctx.WorkflowDefinitions
            .Include(w => w.Steps).ThenInclude(s => s.ApproverRole)
            .FirstOrDefaultAsync(w => w.Id == id && w.OrganizationId == orgId);

    public Task<WorkflowDefinition?> GetActiveForEntityTypeAsync(int orgId, string entityType) =>
        _ctx.WorkflowDefinitions
            .Include(w => w.Steps).ThenInclude(s => s.ApproverRole)
            .FirstOrDefaultAsync(w => w.OrganizationId == orgId && w.EntityType == entityType && w.IsActive);

    public Task<bool> HasInstancesAsync(int workflowDefinitionId) =>
        _ctx.WorkflowInstances.AnyAsync(i => i.WorkflowDefinitionId == workflowDefinitionId);

    public void Add(WorkflowDefinition definition)    => _ctx.WorkflowDefinitions.Add(definition);
    public void Update(WorkflowDefinition definition) => _ctx.WorkflowDefinitions.Update(definition);
    public void Remove(WorkflowDefinition definition) => _ctx.WorkflowDefinitions.Remove(definition);
    public Task SaveChangesAsync()                    => _ctx.SaveChangesAsync();
}
