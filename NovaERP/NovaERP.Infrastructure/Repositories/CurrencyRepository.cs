using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly NovaErpDbContext _ctx;

    public CurrencyRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Currency>> GetAllAsync() =>
        _ctx.Currencies.OrderBy(c => c.Code).ToListAsync();

    public Task<Currency?> GetByCodeAsync(string code) =>
        _ctx.Currencies.FirstOrDefaultAsync(c => c.Code == code);

    public void Add(Currency currency)    => _ctx.Currencies.Add(currency);
    public void Update(Currency currency) => _ctx.Currencies.Update(currency);
    public Task SaveChangesAsync()        => _ctx.SaveChangesAsync();
}
