using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IExchangeRateRepository
{
    Task<List<ExchangeRate>> GetAllByOrgAsync(int orgId);
    Task<ExchangeRate?> GetByIdAsync(int orgId, int id);
    void Add(ExchangeRate rate);
    void Update(ExchangeRate rate);
    void Remove(ExchangeRate rate);
    Task SaveChangesAsync();
}
