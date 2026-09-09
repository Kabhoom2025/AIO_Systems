using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly HrmsDbContext _ctx;

    public OrganizationRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<Organization?> GetByIdAsync(int id) =>
        _ctx.Organizations.FirstOrDefaultAsync(o => o.Id == id);

    public void Add(Organization organization)    => _ctx.Organizations.Add(organization);
    public void Update(Organization organization) => _ctx.Organizations.Update(organization);
    public void Remove(Organization organization) => _ctx.Organizations.Remove(organization);
    public Task SaveChangesAsync()                => _ctx.SaveChangesAsync();
}
