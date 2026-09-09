using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ICurrencyService
{
    Task<List<CurrencyDto>> GetAllAsync();
    Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto);
    Task<CurrencyDto> UpdateAsync(string code, UpdateCurrencyDto dto);
}
