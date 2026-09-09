using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class AutomationRuleService : IAutomationRuleService
{
    private readonly IAutomationRuleRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateAutomationRuleDto> _createValidator;
    private readonly IValidator<UpdateAutomationRuleDto> _updateValidator;

    public AutomationRuleService(IAutomationRuleRepository repo, IMapper mapper,
        IValidator<CreateAutomationRuleDto> createValidator, IValidator<UpdateAutomationRuleDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<AutomationRuleDto>> GetAllAsync(int orgId)
    {
        var rules = await _repo.GetAllByOrgAsync(orgId);
        return rules.Select(_mapper.Map<AutomationRuleDto>).ToList();
    }

    public async Task<AutomationRuleDto> GetByIdAsync(int orgId, int id)
    {
        var rule = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"AutomationRule {id} not found");
        return _mapper.Map<AutomationRuleDto>(rule);
    }

    public async Task<AutomationRuleDto> CreateAsync(int orgId, CreateAutomationRuleDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var rule = _mapper.Map<AutomationRule>(dto);
        rule.OrganizationId = orgId;

        _repo.Add(rule);
        await _repo.SaveChangesAsync();
        return _mapper.Map<AutomationRuleDto>(rule);
    }

    public async Task<AutomationRuleDto> UpdateAsync(int orgId, int id, UpdateAutomationRuleDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var rule = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"AutomationRule {id} not found");

        _mapper.Map(dto, rule);
        rule.UpdatedDate = DateTime.UtcNow;

        _repo.Update(rule);
        await _repo.SaveChangesAsync();
        return _mapper.Map<AutomationRuleDto>(rule);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var rule = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"AutomationRule {id} not found");
        _repo.Remove(rule);
        await _repo.SaveChangesAsync();
    }
}
