using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly NovaErpDbContext _ctx;

    public SettingsRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<OrganizationSettings?> GetByOrgAsync(int orgId) =>
        _ctx.OrganizationSettings.FirstOrDefaultAsync(s => s.OrganizationId == orgId);

    public void Add(OrganizationSettings settings)    => _ctx.OrganizationSettings.Add(settings);
    public void Update(OrganizationSettings settings) => _ctx.OrganizationSettings.Update(settings);
    public Task SaveChangesAsync()                    => _ctx.SaveChangesAsync();
}
