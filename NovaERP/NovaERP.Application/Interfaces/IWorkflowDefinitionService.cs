using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IWorkflowDefinitionService
{
    Task<List<WorkflowDefinitionDto>> GetAllAsync(int orgId);
    Task<WorkflowDefinitionDto> GetByIdAsync(int orgId, int id);
    Task<WorkflowDefinitionDto> CreateAsync(int orgId, CreateWorkflowDefinitionDto dto);
    Task<WorkflowDefinitionDto> UpdateAsync(int orgId, int id, UpdateWorkflowDefinitionDto dto);
    Task DeleteAsync(int orgId, int id);
}
