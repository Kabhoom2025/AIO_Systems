using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly NovaErpDbContext _ctx;

    public LeadRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Lead>> GetAllByOrgAsync(int orgId) =>
        _ctx.Leads
            .Include(l => l.Owner)
            .Include(l => l.ConvertedAccount)
            .Where(l => l.OrganizationId == orgId)
            .OrderByDescending(l => l.CreatedDate)
            .ToListAsync();

    public Task<Lead?> GetByIdAsync(int orgId, int id) =>
        _ctx.Leads
            .Include(l => l.Owner)
            .Include(l => l.ConvertedAccount)
            .FirstOrDefaultAsync(l => l.Id == id && l.OrganizationId == orgId);

    public void Add(Lead lead)    => _ctx.Leads.Add(lead);
    public void Update(Lead lead) => _ctx.Leads.Update(lead);
    public void Remove(Lead lead) => _ctx.Leads.Remove(lead);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
