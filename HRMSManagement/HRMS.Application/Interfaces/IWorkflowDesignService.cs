using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

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
}
