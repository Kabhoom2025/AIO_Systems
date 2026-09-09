using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IAutomationRuleRepository
{
    Task<List<AutomationRule>> GetAllByOrgAsync(int orgId);
    Task<AutomationRule?> GetByIdAsync(int orgId, int id);
    Task<List<AutomationRule>> GetEnabledForEventAsync(int orgId, string triggerEvent);
    void Add(AutomationRule rule);
    void Update(AutomationRule rule);
    void Remove(AutomationRule rule);
    Task SaveChangesAsync();
}
