using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IAutomationRuleService
{
    Task<List<AutomationRuleDto>> GetAllAsync(int orgId);
    Task<AutomationRuleDto> GetByIdAsync(int orgId, int id);
    Task<AutomationRuleDto> CreateAsync(int orgId, CreateAutomationRuleDto dto);
    Task<AutomationRuleDto> UpdateAsync(int orgId, int id, UpdateAutomationRuleDto dto);
    Task DeleteAsync(int orgId, int id);
}
