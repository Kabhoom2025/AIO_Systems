using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IAccountService
{
    Task<List<AccountDto>> GetAllAsync(int orgId);
    Task<AccountDto> GetByIdAsync(int orgId, int id);
    Task<AccountDto> CreateAsync(int orgId, CreateAccountDto dto);
    Task<AccountDto> UpdateAsync(int orgId, int id, UpdateAccountDto dto);
    Task DeleteAsync(int orgId, int id);
}
