using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IWorkflowDefinitionRepository
{
    Task<List<WorkflowDefinition>> GetAllByOrgAsync(int orgId);
    Task<WorkflowDefinition?> GetByIdAsync(int orgId, int id);
    Task<WorkflowDefinition?> GetActiveForEntityTypeAsync(int orgId, string entityType);
    Task<bool> HasInstancesAsync(int workflowDefinitionId);
    void Add(WorkflowDefinition definition);
    void Update(WorkflowDefinition definition);
    void Remove(WorkflowDefinition definition);
    Task SaveChangesAsync();
}
