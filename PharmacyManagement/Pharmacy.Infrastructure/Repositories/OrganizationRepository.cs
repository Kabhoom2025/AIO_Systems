using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly PharmacyDbContext _ctx;

    public OrganizationRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<Organization?> GetByIdAsync(int id) =>
        _ctx.Organizations.FirstOrDefaultAsync(o => o.Id == id);

    public Task<List<int>> GetAllActiveIdsAsync() =>
        _ctx.Organizations.Where(o => o.IsActive).Select(o => o.Id).ToListAsync();

    public void Update(Organization organization) => _ctx.Organizations.Update(organization);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
