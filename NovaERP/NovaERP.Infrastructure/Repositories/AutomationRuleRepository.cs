using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class AutomationRuleRepository : IAutomationRuleRepository
{
    private readonly NovaErpDbContext _ctx;

    public AutomationRuleRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<AutomationRule>> GetAllByOrgAsync(int orgId) =>
        _ctx.AutomationRules
            .Include(a => a.NotifyRole)
            .Where(a => a.OrganizationId == orgId)
            .OrderBy(a => a.Name)
            .ToListAsync();

    public Task<AutomationRule?> GetByIdAsync(int orgId, int id) =>
        _ctx.AutomationRules
            .Include(a => a.NotifyRole)
            .FirstOrDefaultAsync(a => a.Id == id && a.OrganizationId == orgId);

    public Task<List<AutomationRule>> GetEnabledForEventAsync(int orgId, string triggerEvent) =>
        _ctx.AutomationRules
            .Where(a => a.OrganizationId == orgId && a.TriggerEvent == triggerEvent && a.IsEnabled)
            .ToListAsync();

    public void Add(AutomationRule rule)    => _ctx.AutomationRules.Add(rule);
    public void Update(AutomationRule rule) => _ctx.AutomationRules.Update(rule);
    public void Remove(AutomationRule rule) => _ctx.AutomationRules.Remove(rule);
    public Task SaveChangesAsync()          => _ctx.SaveChangesAsync();
}
