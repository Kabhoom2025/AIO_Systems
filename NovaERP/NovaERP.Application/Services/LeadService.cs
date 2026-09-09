using FluentValidation;
using NovaERP.Application.Common;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly IContactRepository _contactRepo;
    private readonly IOpportunityRepository _opportunityRepo;
    private readonly IAutomationEngine _automationEngine;
    private readonly IValidator<CreateLeadDto> _createValidator;
    private readonly IValidator<UpdateLeadDto> _updateValidator;
    private readonly IValidator<ConvertLeadDto> _convertValidator;

    public LeadService(ILeadRepository leadRepo, IAccountRepository accountRepo, IContactRepository contactRepo,
        IOpportunityRepository opportunityRepo, IAutomationEngine automationEngine,
        IValidator<CreateLeadDto> createValidator, IValidator<UpdateLeadDto> updateValidator,
        IValidator<ConvertLeadDto> convertValidator)
    {
        _leadRepo = leadRepo;
        _accountRepo = accountRepo;
        _contactRepo = contactRepo;
        _opportunityRepo = opportunityRepo;
        _automationEngine = automationEngine;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _convertValidator = convertValidator;
    }

    public async Task<List<LeadDto>> GetAllAsync(int orgId)
    {
        var leads = await _leadRepo.GetAllByOrgAsync(orgId);
        return leads.Select(ToDto).ToList();
    }

    public async Task<LeadDto> GetByIdAsync(int orgId, int id)
    {
        var lead = await _leadRepo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Lead {id} not found");
        return ToDto(lead);
    }

    public async Task<LeadDto> CreateAsync(int orgId, CreateLeadDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var lead = new Lead
        {
            OrganizationId = orgId,
            Name = dto.Name,
            CompanyName = dto.CompanyName,
            Email = dto.Email,
            Phone = dto.Phone,
            Source = dto.Source,
            Status = "New",
            OwnerId = dto.OwnerId
        };

        _leadRepo.Add(lead);
        await _leadRepo.SaveChangesAsync();

        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.LeadCreated, new Dictionary<string, string>
        {
            ["EntityType"] = "Lead",
            ["EntityId"] = lead.Id.ToString(),
            ["Name"] = lead.Name
        });

        return ToDto(lead);
    }

    public async Task<LeadDto> UpdateAsync(int orgId, int id, UpdateLeadDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var lead = await _leadRepo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Lead {id} not found");

        if (lead.Status == "Converted")
            throw new InvalidOperationException("A converted lead cannot be edited.");

        lead.Name = dto.Name;
        lead.CompanyName = dto.CompanyName;
        lead.Email = dto.Email;
        lead.Phone = dto.Phone;
        lead.Source = dto.Source;
        lead.Status = dto.Status;
        lead.OwnerId = dto.OwnerId;
        lead.UpdatedDate = DateTime.UtcNow;

        _leadRepo.Update(lead);
        await _leadRepo.SaveChangesAsync();
        return ToDto(lead);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var lead = await _leadRepo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Lead {id} not found");
        _leadRepo.Remove(lead);
        await _leadRepo.SaveChangesAsync();
    }

    public async Task<ConvertLeadResultDto> ConvertAsync(int orgId, int id, ConvertLeadDto dto)
    {
        await _convertValidator.ValidateAndThrowAsync(dto);

        var lead = await _leadRepo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Lead {id} not found");

        if (lead.Status == "Converted")
            throw new InvalidOperationException("This lead has already been converted.");

        var account = new Account
        {
            OrganizationId = orgId,
            Name = dto.AccountName ?? lead.CompanyName ?? lead.Name,
            OwnerId = lead.OwnerId
        };
        _accountRepo.Add(account);

        // Lead.Name is a single free-text field — split on first space for the new
        // Contact's First/Last name, same simplification a quick "convert" action needs.
        var nameParts = lead.Name.Split(' ', 2);
        var contact = new Contact
        {
            OrganizationId = orgId,
            Account = account,
            FirstName = nameParts[0],
            LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
            Email = lead.Email,
            Phone = lead.Phone,
            OwnerId = lead.OwnerId
        };
        _contactRepo.Add(contact);

        Opportunity? opportunity = null;
        if (dto.CreateOpportunity)
        {
            opportunity = new Opportunity
            {
                OrganizationId = orgId,
                Account = account,
                Name = dto.OpportunityName!,
                Amount = dto.OpportunityAmount!.Value,
                Stage = "Qualification",
                OwnerId = lead.OwnerId
            };
            _opportunityRepo.Add(opportunity);
        }

        lead.Status = "Converted";
        lead.ConvertedAccount = account;
        lead.ConvertedDate = DateTime.UtcNow;
        lead.UpdatedDate = DateTime.UtcNow;
        _leadRepo.Update(lead);

        await _leadRepo.SaveChangesAsync();

        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.LeadConverted, new Dictionary<string, string>
        {
            ["EntityType"] = "Lead",
            ["EntityId"] = lead.Id.ToString(),
            ["AccountId"] = account.Id.ToString()
        });

        return new ConvertLeadResultDto
        {
            LeadId = lead.Id,
            AccountId = account.Id,
            ContactId = contact.Id,
            OpportunityId = opportunity?.Id
        };
    }

    private static LeadDto ToDto(Lead l) => new()
    {
        Id = l.Id,
        Name = l.Name,
        CompanyName = l.CompanyName,
        Email = l.Email,
        Phone = l.Phone,
        Source = l.Source,
        Status = l.Status,
        OwnerId = l.OwnerId,
        OwnerName = l.Owner?.Name ?? string.Empty,
        ConvertedAccountId = l.ConvertedAccountId,
        ConvertedAccountName = l.ConvertedAccount?.Name,
        ConvertedDate = l.ConvertedDate
    };
}
