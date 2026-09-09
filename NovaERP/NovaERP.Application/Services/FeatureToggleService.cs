using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class FeatureToggleService : IFeatureToggleService
{
    private readonly IFeatureToggleRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateFeatureToggleDto> _updateValidator;

    public FeatureToggleService(IFeatureToggleRepository repo, IMapper mapper, IValidator<UpdateFeatureToggleDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _updateValidator = updateValidator;
    }

    public async Task<List<FeatureToggleDto>> GetAllAsync(int orgId)
    {
        var toggles = await _repo.GetAllByOrgAsync(orgId);
        return toggles.Select(_mapper.Map<FeatureToggleDto>).ToList();
    }

    public async Task<FeatureToggleDto> UpdateAsync(int orgId, string moduleKey, UpdateFeatureToggleDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var toggle = await _repo.GetByModuleKeyAsync(orgId, moduleKey);
        if (toggle == null)
        {
            toggle = new FeatureToggle { OrganizationId = orgId, ModuleKey = moduleKey, IsEnabled = dto.IsEnabled };
            _repo.Add(toggle);
        }
        else
        {
            toggle.IsEnabled = dto.IsEnabled;
            toggle.UpdatedDate = DateTime.UtcNow;
            _repo.Update(toggle);
        }

        await _repo.SaveChangesAsync();
        return _mapper.Map<FeatureToggleDto>(toggle);
    }
}
