using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IFeatureToggleRepository
{
    Task<List<FeatureToggle>> GetAllByOrgAsync(int orgId);
    Task<FeatureToggle?> GetByModuleKeyAsync(int orgId, string moduleKey);
    void Add(FeatureToggle toggle);
    void Update(FeatureToggle toggle);
    Task SaveChangesAsync();
}
