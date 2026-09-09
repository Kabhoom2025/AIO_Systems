using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class OpportunityRepository : IOpportunityRepository
{
    private readonly NovaErpDbContext _ctx;

    public OpportunityRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Opportunity>> GetAllByOrgAsync(int orgId) =>
        _ctx.Opportunities
            .Include(o => o.Owner)
            .Include(o => o.Account)
            .Where(o => o.OrganizationId == orgId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();

    public Task<Opportunity?> GetByIdAsync(int orgId, int id) =>
        _ctx.Opportunities
            .Include(o => o.Owner)
            .Include(o => o.Account)
            .FirstOrDefaultAsync(o => o.Id == id && o.OrganizationId == orgId);

    public void Add(Opportunity opportunity)    => _ctx.Opportunities.Add(opportunity);
    public void Update(Opportunity opportunity) => _ctx.Opportunities.Update(opportunity);
    public void Remove(Opportunity opportunity) => _ctx.Opportunities.Remove(opportunity);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
