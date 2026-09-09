using FluentValidation;
using NovaERP.Application.Common;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class OpportunityService : IOpportunityService
{
    private readonly IOpportunityRepository _repo;
    private readonly IAutomationEngine _automationEngine;
    private readonly IValidator<CreateOpportunityDto> _createValidator;
    private readonly IValidator<UpdateOpportunityDto> _updateValidator;

    public OpportunityService(IOpportunityRepository repo, IAutomationEngine automationEngine,
        IValidator<CreateOpportunityDto> createValidator, IValidator<UpdateOpportunityDto> updateValidator)
    {
        _repo = repo;
        _automationEngine = automationEngine;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<OpportunityDto>> GetAllAsync(int orgId)
    {
        var opportunities = await _repo.GetAllByOrgAsync(orgId);
        return opportunities.Select(ToDto).ToList();
    }

    public async Task<OpportunityDto> GetByIdAsync(int orgId, int id)
    {
        var opportunity = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Opportunity {id} not found");
        return ToDto(opportunity);
    }

    public async Task<OpportunityDto> CreateAsync(int orgId, CreateOpportunityDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var opportunity = new Opportunity
        {
            OrganizationId = orgId,
            AccountId = dto.AccountId,
            Name = dto.Name,
            Amount = dto.Amount,
            Stage = dto.Stage,
            CloseDate = dto.CloseDate,
            OwnerId = dto.OwnerId
        };

        _repo.Add(opportunity);
        await _repo.SaveChangesAsync();
        return ToDto(opportunity);
    }

    public async Task<OpportunityDto> UpdateAsync(int orgId, int id, UpdateOpportunityDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var opportunity = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Opportunity {id} not found");

        var previousStage = opportunity.Stage;

        opportunity.Name = dto.Name;
        opportunity.Amount = dto.Amount;
        opportunity.Stage = dto.Stage;
        opportunity.CloseDate = dto.CloseDate;
        opportunity.OwnerId = dto.OwnerId;
        opportunity.UpdatedDate = DateTime.UtcNow;

        _repo.Update(opportunity);
        await _repo.SaveChangesAsync();

        if (previousStage != "Won" && dto.Stage == "Won")
        {
            await _automationEngine.HandleEventAsync(orgId, AutomationEvents.OpportunityWon, new Dictionary<string, string>
            {
                ["EntityType"] = "Opportunity",
                ["EntityId"] = opportunity.Id.ToString(),
                ["Amount"] = opportunity.Amount.ToString("F2")
            });
        }
        else if (previousStage != "Lost" && dto.Stage == "Lost")
        {
            await _automationEngine.HandleEventAsync(orgId, AutomationEvents.OpportunityLost, new Dictionary<string, string>
            {
                ["EntityType"] = "Opportunity",
                ["EntityId"] = opportunity.Id.ToString()
            });
        }

        return ToDto(opportunity);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var opportunity = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Opportunity {id} not found");
        _repo.Remove(opportunity);
        await _repo.SaveChangesAsync();
    }

    private static OpportunityDto ToDto(Opportunity o) => new()
    {
        Id = o.Id,
        AccountId = o.AccountId,
        AccountName = o.Account?.Name ?? string.Empty,
        Name = o.Name,
        Amount = o.Amount,
        Stage = o.Stage,
        CloseDate = o.CloseDate,
        OwnerId = o.OwnerId,
        OwnerName = o.Owner?.Name ?? string.Empty
    };
}
