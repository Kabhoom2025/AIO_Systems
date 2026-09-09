using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly NovaErpDbContext _ctx;

    public OrganizationRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<Organization?> GetByIdAsync(int id) =>
        _ctx.Organizations.FirstOrDefaultAsync(o => o.Id == id);

    public void Add(Organization organization)    => _ctx.Organizations.Add(organization);
    public void Update(Organization organization) => _ctx.Organizations.Update(organization);
    public void Remove(Organization organization) => _ctx.Organizations.Remove(organization);
    public Task SaveChangesAsync()                => _ctx.SaveChangesAsync();
}
