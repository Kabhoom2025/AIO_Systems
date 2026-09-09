using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateOrganizationSettingsDto> _updateValidator;

    public SettingsService(ISettingsRepository repo, IMapper mapper, IValidator<UpdateOrganizationSettingsDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _updateValidator = updateValidator;
    }

    public async Task<OrganizationSettingsDto> GetAsync(int orgId)
    {
        var settings = await GetOrCreateAsync(orgId);
        return _mapper.Map<OrganizationSettingsDto>(settings);
    }

    public async Task<OrganizationSettingsDto> UpdateAsync(int orgId, UpdateOrganizationSettingsDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var settings = await GetOrCreateAsync(orgId);
        _mapper.Map(dto, settings);
        settings.UpdatedDate = DateTime.UtcNow;

        _repo.Update(settings);
        await _repo.SaveChangesAsync();
        return _mapper.Map<OrganizationSettingsDto>(settings);
    }

    /// <summary>Every org gets a settings row lazily on first access rather than requiring a
    /// separate provisioning step when the org is created.</summary>
    private async Task<OrganizationSettings> GetOrCreateAsync(int orgId)
    {
        var settings = await _repo.GetByOrgAsync(orgId);
        if (settings != null) return settings;

        settings = new OrganizationSettings { OrganizationId = orgId };
        _repo.Add(settings);
        await _repo.SaveChangesAsync();
        return settings;
    }
}
