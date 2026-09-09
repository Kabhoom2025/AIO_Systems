using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ICurrencyRepository
{
    Task<List<Currency>> GetAllAsync();
    Task<Currency?> GetByCodeAsync(string code);
    void Add(Currency currency);
    void Update(Currency currency);
    Task SaveChangesAsync();
}
