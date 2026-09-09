using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IFeatureToggleService
{
    Task<List<FeatureToggleDto>> GetAllAsync(int orgId);
    Task<FeatureToggleDto> UpdateAsync(int orgId, string moduleKey, UpdateFeatureToggleDto dto);
}
