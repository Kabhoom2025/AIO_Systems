using Workflow.Application.DTOs;

namespace Workflow.Application.Interfaces;

public interface IWorkflowDesignService
{
    Task<List<WorkflowDefinitionDto>> GetAllAsync(int orgId);
    Task<WorkflowDefinitionDto> GetByIdAsync(int orgId, int id);
    Task<WorkflowDefinitionDto> CreateAsync(int orgId, CreateWorkflowDefinitionDto dto);
    Task<WorkflowDefinitionDto> UpdateAsync(int orgId, int id, UpdateWorkflowDefinitionDto dto);
    Task DeleteAsync(int orgId, int id);

    Task<List<WorkflowVersionDto>> GetVersionsAsync(int orgId, int id);
    Task<WorkflowVersionDto> GetDraftAsync(int orgId, int id);
    Task<WorkflowVersionDto> SaveDraftAsync(int orgId, int id, SaveDraftGraphDto dto);
    Task<WorkflowDefinitionDto> PublishAsync(int orgId, int id, int userId);

    List<TriggerTypeDto> GetTriggerTypes();
    List<ActionTypeDto> GetActionTypes();

    Task<List<WorkflowExecutionDto>> GetExecutionsAsync(int orgId, int id);

    /// <summary>Test-runs the current draft graph with caller-supplied context. Pass <paramref name="onStep"/>
    /// to receive each step as it happens (used by the SSE streaming endpoint).</summary>
    Task<WorkflowExecutionDto> RunDraftAsync(int orgId, int id, RunWorkflowDto dto, Func<Services.WorkflowStepEvent, Task>? onStep = null);

    /// <summary>Runs the published graph for the workflow owning this webhook token.
    /// Returns null if no workflow has this token or it has nothing published.</summary>
    Task<WorkflowExecutionDto?> RunByWebhookAsync(string token, RunWorkflowDto dto);
}
