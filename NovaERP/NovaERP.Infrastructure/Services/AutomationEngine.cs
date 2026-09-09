using Microsoft.EntityFrameworkCore;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

/// <summary>Looks up enabled AutomationRules for an org+event and executes each one. Only
/// ActionType == "Notify" is implemented in this phase (in-app notifications); any other
/// ActionType value is a documented no-op extension point for the multi-channel notification
/// work another team is doing in parallel — it must never throw.</summary>
public class AutomationEngine : IAutomationEngine
{
    private readonly NovaErpDbContext _ctx;
    private readonly IAutomationRuleRepository _ruleRepo;
    private readonly INotificationService _notificationService;

    public AutomationEngine(NovaErpDbContext ctx, IAutomationRuleRepository ruleRepo, INotificationService notificationService)
    {
        _ctx = ctx;
        _ruleRepo = ruleRepo;
        _notificationService = notificationService;
    }

    public async Task HandleEventAsync(int organizationId, string triggerEvent, IDictionary<string, string> context)
    {
        var rules = await _ruleRepo.GetEnabledForEventAsync(organizationId, triggerEvent);
        if (rules.Count == 0) return;

        foreach (var rule in rules)
        {
            if (rule.ActionType != "Notify")
                continue; // Extension point: Email/SMS/Push are handled by the parallel Scheduler/Notifications workstream.

            if (rule.NotifyRoleId == null) continue;

            var message = rule.NotifyMessageTemplate;
            foreach (var kvp in context)
                message = message.Replace("{" + kvp.Key + "}", kvp.Value);

            var recipientUserIds = await _ctx.Users
                .Where(u => u.OrganizationId == organizationId && u.RoleId == rule.NotifyRoleId && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in recipientUserIds)
            {
                await _notificationService.CreateAsync(organizationId, new CreateNotificationDto
                {
                    UserId = userId,
                    Title = rule.Name,
                    Message = message,
                    Type = "Info"
                });
            }
        }
    }
}
