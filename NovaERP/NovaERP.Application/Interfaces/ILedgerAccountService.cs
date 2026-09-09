using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ILedgerAccountService
{
    Task<List<LedgerAccountDto>> GetAllAsync(int orgId);
    Task<LedgerAccountDto> GetByIdAsync(int orgId, int id);
    Task<LedgerAccountDto> CreateAsync(int orgId, CreateLedgerAccountDto dto);
    Task<LedgerAccountDto> UpdateAsync(int orgId, int id, UpdateLedgerAccountDto dto);
    Task DeleteAsync(int orgId, int id);
}
