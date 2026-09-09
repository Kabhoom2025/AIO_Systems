using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class OrganizationLanguageRepository : IOrganizationLanguageRepository
{
    private readonly NovaErpDbContext _ctx;

    public OrganizationLanguageRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<OrganizationLanguage>> GetAllByOrgAsync(int orgId) =>
        _ctx.OrganizationLanguages
            .Where(o => o.OrganizationId == orgId)
            .OrderBy(o => o.LanguageCode)
            .ToListAsync();

    public void AddRange(IEnumerable<OrganizationLanguage> items)    => _ctx.OrganizationLanguages.AddRange(items);
    public void RemoveRange(IEnumerable<OrganizationLanguage> items) => _ctx.OrganizationLanguages.RemoveRange(items);
    public Task SaveChangesAsync()                                  => _ctx.SaveChangesAsync();
}
