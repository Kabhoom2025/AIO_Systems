using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IWorkflowRepository
{
    Task<List<WorkflowDefinition>> GetAllAsync(int orgId);
    Task<WorkflowDefinition?> GetByIdAsync(int orgId, int id);
    Task<WorkflowDefinition?> GetByIdWithVersionsAsync(int orgId, int id);
    void AddDefinition(WorkflowDefinition definition);
    void UpdateDefinition(WorkflowDefinition definition);
    void RemoveDefinition(WorkflowDefinition definition);

    Task<WorkflowVersion?> GetDraftVersionAsync(int workflowDefinitionId);
    Task<WorkflowVersion?> GetVersionAsync(int workflowDefinitionId, int versionId);
    Task<List<WorkflowVersion>> GetVersionsAsync(int workflowDefinitionId);
    void AddVersion(WorkflowVersion version);
    void UpdateVersion(WorkflowVersion version);

    /// <summary>The single active Published workflow for a given org + trigger key, if any.</summary>
    Task<(WorkflowDefinition Definition, WorkflowVersion Version)?> GetPublishedForTriggerAsync(int orgId, string triggerType);

    void AddExecution(WorkflowExecution execution);
    Task<List<WorkflowExecution>> GetExecutionsAsync(int orgId, int workflowDefinitionId, int take = 50);

    Task SaveChangesAsync();
}
