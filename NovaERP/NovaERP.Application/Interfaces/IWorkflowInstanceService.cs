using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IWorkflowInstanceService
{
    Task<WorkflowInstanceDto> StartAsync(int orgId, int submittedByUserId, StartWorkflowDto dto);
    Task<WorkflowInstanceDto> ApproveAsync(int orgId, int instanceId, int actingUserId, WorkflowActionDto dto);
    Task<WorkflowInstanceDto> RejectAsync(int orgId, int instanceId, int actingUserId, WorkflowActionDto dto);
    Task<List<WorkflowInstanceDto>> GetPendingForUserAsync(int orgId, int userId);
    Task<List<WorkflowInstanceDto>> GetByEntityAsync(int orgId, string entityType, int entityId);
}
