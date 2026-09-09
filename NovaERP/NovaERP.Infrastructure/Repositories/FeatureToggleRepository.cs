using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class FeatureToggleRepository : IFeatureToggleRepository
{
    private readonly NovaErpDbContext _ctx;

    public FeatureToggleRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<FeatureToggle>> GetAllByOrgAsync(int orgId) =>
        _ctx.FeatureToggles
            .Where(f => f.OrganizationId == orgId)
            .OrderBy(f => f.ModuleKey)
            .ToListAsync();

    public Task<FeatureToggle?> GetByModuleKeyAsync(int orgId, string moduleKey) =>
        _ctx.FeatureToggles.FirstOrDefaultAsync(f => f.OrganizationId == orgId && f.ModuleKey == moduleKey);

    public void Add(FeatureToggle toggle)    => _ctx.FeatureToggles.Add(toggle);
    public void Update(FeatureToggle toggle) => _ctx.FeatureToggles.Update(toggle);
    public Task SaveChangesAsync()           => _ctx.SaveChangesAsync();
}
