using Workflow.Domain.Entities;

namespace Workflow.Application.Interfaces;

public interface IWorkflowRepository
{
    Task<List<WorkflowDefinition>> GetAllAsync(int orgId);
    Task<WorkflowDefinition?> GetByIdAsync(int orgId, int id);
    Task<WorkflowDefinition?> GetByWebhookTokenAsync(string token);
    void AddDefinition(WorkflowDefinition definition);
    void UpdateDefinition(WorkflowDefinition definition);
    void RemoveDefinition(WorkflowDefinition definition);

    Task<WorkflowVersion?> GetDraftVersionAsync(int workflowDefinitionId);
    Task<WorkflowVersion?> GetVersionAsync(int workflowDefinitionId, int versionId);
    Task<List<WorkflowVersion>> GetVersionsAsync(int workflowDefinitionId);
    void AddVersion(WorkflowVersion version);
    void UpdateVersion(WorkflowVersion version);

    void AddExecution(WorkflowExecution execution);
    Task<List<WorkflowExecution>> GetExecutionsAsync(int orgId, int workflowDefinitionId, int take = 50);

    Task SaveChangesAsync();
}
