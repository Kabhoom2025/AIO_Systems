using System.Text.Json;
using LinkShield.Application.DTOs.Admin;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Entities;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Services;

public class RiskRuleManagementService : IRiskRuleManagementService
{
    private readonly LinkShieldDbContext _db;

    public RiskRuleManagementService(LinkShieldDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RiskRuleDto>> GetAllAsync(CancellationToken ct = default) =>
        await _db.RiskRules
            .OrderBy(r => r.Category).ThenBy(r => r.Name)
            .Select(r => new RiskRuleDto(r.Id, r.Name, r.Description, r.Category, r.Weight, r.IsEnabled, r.ConditionExpression, r.ScoreContribution))
            .ToListAsync(ct);

    public async Task<RiskRuleDto> CreateAsync(CreateRiskRuleRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        if (await _db.RiskRules.AnyAsync(r => r.Name == request.Name, ct))
            throw new InvalidOperationException($"A risk rule named '{request.Name}' already exists.");

        var rule = new RiskRule
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Weight = request.Weight,
            ConditionExpression = request.ConditionExpression,
            ScoreContribution = request.ScoreContribution,
            IsEnabled = true
        };
        _db.RiskRules.Add(rule);

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = actingUserId,
            Action = AuditAction.RiskRuleChanged,
            EntityType = nameof(RiskRule),
            EntityId = rule.Id.ToString(),
            DetailsJson = JsonSerializer.Serialize(new { action = "created", rule.Name, rule.Weight, rule.ScoreContribution })
        });

        await _db.SaveChangesAsync(ct);

        return new RiskRuleDto(rule.Id, rule.Name, rule.Description, rule.Category, rule.Weight, rule.IsEnabled, rule.ConditionExpression, rule.ScoreContribution);
    }

    public async Task<RiskRuleDto> UpdateAsync(Guid id, UpdateRiskRuleRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var rule = await _db.RiskRules.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Risk rule '{id}' not found.");

        rule.Name = request.Name;
        rule.Description = request.Description;
        rule.Weight = request.Weight;
        rule.IsEnabled = request.IsEnabled;
        rule.ConditionExpression = request.ConditionExpression;
        rule.ScoreContribution = request.ScoreContribution;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = actingUserId,
            Action = AuditAction.RiskRuleChanged,
            EntityType = nameof(RiskRule),
            EntityId = rule.Id.ToString(),
            DetailsJson = JsonSerializer.Serialize(new { action = "updated", rule.Name, rule.Weight, rule.IsEnabled, rule.ScoreContribution })
        });

        await _db.SaveChangesAsync(ct);

        return new RiskRuleDto(rule.Id, rule.Name, rule.Description, rule.Category, rule.Weight, rule.IsEnabled, rule.ConditionExpression, rule.ScoreContribution);
    }
}
