using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ITaxCodeService
{
    Task<List<TaxCodeDto>> GetAllAsync(int orgId);
    Task<TaxCodeDto> GetByIdAsync(int orgId, int id);
    Task<TaxCodeDto> CreateAsync(int orgId, CreateTaxCodeDto dto);
    Task<TaxCodeDto> UpdateAsync(int orgId, int id, UpdateTaxCodeDto dto);
    Task DeleteAsync(int orgId, int id);
}
