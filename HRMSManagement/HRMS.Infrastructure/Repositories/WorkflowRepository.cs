using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class WorkflowRepository : IWorkflowRepository
{
    private readonly HrmsDbContext _ctx;

    public WorkflowRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<WorkflowDefinition>> GetAllAsync(int orgId) =>
        _ctx.WorkflowDefinitions
            .Include(w => w.PublishedVersion)
            .Where(w => w.OrganizationId == orgId)
            .OrderByDescending(w => w.UpdatedDate ?? w.CreatedDate)
            .ToListAsync();

    public Task<WorkflowDefinition?> GetByIdAsync(int orgId, int id) =>
        _ctx.WorkflowDefinitions
            .Include(w => w.PublishedVersion)
            .FirstOrDefaultAsync(w => w.OrganizationId == orgId && w.Id == id);

    public Task<WorkflowDefinition?> GetByIdWithVersionsAsync(int orgId, int id) =>
        _ctx.WorkflowDefinitions
            .Include(w => w.PublishedVersion)
            .Include(w => w.Versions)
            .FirstOrDefaultAsync(w => w.OrganizationId == orgId && w.Id == id);

    public void AddDefinition(WorkflowDefinition definition) => _ctx.WorkflowDefinitions.Add(definition);
    public void UpdateDefinition(WorkflowDefinition definition) => _ctx.WorkflowDefinitions.Update(definition);
    public void RemoveDefinition(WorkflowDefinition definition) => _ctx.WorkflowDefinitions.Remove(definition);

    public Task<WorkflowVersion?> GetDraftVersionAsync(int workflowDefinitionId) =>
        _ctx.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == workflowDefinitionId && v.Status == "Draft")
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync();

    public Task<WorkflowVersion?> GetVersionAsync(int workflowDefinitionId, int versionId) =>
        _ctx.WorkflowVersions
            .Include(v => v.PublishedByUser)
            .FirstOrDefaultAsync(v => v.WorkflowDefinitionId == workflowDefinitionId && v.Id == versionId);

    public Task<List<WorkflowVersion>> GetVersionsAsync(int workflowDefinitionId) =>
        _ctx.WorkflowVersions
            .Include(v => v.PublishedByUser)
            .Where(v => v.WorkflowDefinitionId == workflowDefinitionId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

    public void AddVersion(WorkflowVersion version) => _ctx.WorkflowVersions.Add(version);
    public void UpdateVersion(WorkflowVersion version) => _ctx.WorkflowVersions.Update(version);

    public async Task<(WorkflowDefinition Definition, WorkflowVersion Version)?> GetPublishedForTriggerAsync(int orgId, string triggerType)
    {
        var definition = await _ctx.WorkflowDefinitions
            .Include(w => w.PublishedVersion)
            .FirstOrDefaultAsync(w => w.OrganizationId == orgId && w.TriggerType == triggerType
                                      && w.IsActive && w.PublishedVersionId != null);

        return definition?.PublishedVersion == null ? null : (definition, definition.PublishedVersion);
    }

    public void AddExecution(WorkflowExecution execution) => _ctx.WorkflowExecutions.Add(execution);

    public Task<List<WorkflowExecution>> GetExecutionsAsync(int orgId, int workflowDefinitionId, int take = 50) =>
        _ctx.WorkflowExecutions
            .Where(e => e.OrganizationId == orgId && e.WorkflowDefinitionId == workflowDefinitionId)
            .OrderByDescending(e => e.CreatedDate)
            .Take(take)
            .ToListAsync();

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
