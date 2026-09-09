using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IExchangeRateService
{
    Task<List<ExchangeRateDto>> GetAllAsync(int orgId);
    Task<ExchangeRateDto> GetByIdAsync(int orgId, int id);
    Task<ExchangeRateDto> CreateAsync(int orgId, CreateExchangeRateDto dto);
    Task<ExchangeRateDto> UpdateAsync(int orgId, int id, UpdateExchangeRateDto dto);
    Task DeleteAsync(int orgId, int id);
}
