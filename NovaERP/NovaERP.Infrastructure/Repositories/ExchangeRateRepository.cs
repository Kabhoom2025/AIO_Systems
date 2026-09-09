using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly NovaErpDbContext _ctx;

    public ExchangeRateRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<ExchangeRate>> GetAllByOrgAsync(int orgId) =>
        _ctx.ExchangeRates
            .Where(r => r.OrganizationId == orgId)
            .OrderByDescending(r => r.EffectiveDate)
            .ToListAsync();

    public Task<ExchangeRate?> GetByIdAsync(int orgId, int id) =>
        _ctx.ExchangeRates.FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == orgId);

    public void Add(ExchangeRate rate)    => _ctx.ExchangeRates.Add(rate);
    public void Update(ExchangeRate rate) => _ctx.ExchangeRates.Update(rate);
    public void Remove(ExchangeRate rate) => _ctx.ExchangeRates.Remove(rate);
    public Task SaveChangesAsync()        => _ctx.SaveChangesAsync();
}
