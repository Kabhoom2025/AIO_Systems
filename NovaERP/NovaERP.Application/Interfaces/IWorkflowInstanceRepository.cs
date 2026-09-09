using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IWorkflowInstanceRepository
{
    Task<WorkflowInstance?> GetByIdAsync(int id);
    Task<List<WorkflowInstance>> GetPendingForRoleAsync(int orgId, int roleId);
    Task<List<WorkflowInstance>> GetByEntityAsync(int orgId, string entityType, int entityId);
    void Add(WorkflowInstance instance);
    void Update(WorkflowInstance instance);
    Task SaveChangesAsync();
}
