using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class LedgerAccountService : ILedgerAccountService
{
    private readonly ILedgerAccountRepository _repo;
    private readonly IValidator<CreateLedgerAccountDto> _createValidator;
    private readonly IValidator<UpdateLedgerAccountDto> _updateValidator;

    public LedgerAccountService(ILedgerAccountRepository repo,
        IValidator<CreateLedgerAccountDto> createValidator, IValidator<UpdateLedgerAccountDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<LedgerAccountDto>> GetAllAsync(int orgId)
    {
        var accounts = await _repo.GetAllByOrgAsync(orgId);
        var balances = await _repo.GetBalancesByOrgAsync(orgId);
        return accounts.Select(a => ToDto(a, balances.GetValueOrDefault(a.Id))).ToList();
    }

    public async Task<LedgerAccountDto> GetByIdAsync(int orgId, int id)
    {
        var account = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LedgerAccount {id} not found");
        var balance = await _repo.GetBalanceAsync(orgId, id);
        return ToDto(account, balance);
    }

    public async Task<LedgerAccountDto> CreateAsync(int orgId, CreateLedgerAccountDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.CodeExistsAsync(orgId, code))
            throw new InvalidOperationException($"Ledger account code {code} already exists");

        var account = new LedgerAccount
        {
            OrganizationId = orgId,
            Code = code,
            Name = dto.Name,
            Type = dto.Type,
            IsActive = dto.IsActive
        };

        _repo.Add(account);
        await _repo.SaveChangesAsync();
        return ToDto(account, 0);
    }

    public async Task<LedgerAccountDto> UpdateAsync(int orgId, int id, UpdateLedgerAccountDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var account = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LedgerAccount {id} not found");

        account.Name = dto.Name;
        account.Type = dto.Type;
        account.IsActive = dto.IsActive;
        account.UpdatedDate = DateTime.UtcNow;

        _repo.Update(account);
        await _repo.SaveChangesAsync();

        var balance = await _repo.GetBalanceAsync(orgId, id);
        return ToDto(account, balance);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var account = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LedgerAccount {id} not found");
        _repo.Remove(account);
        await _repo.SaveChangesAsync();
    }

    private static LedgerAccountDto ToDto(LedgerAccount a, decimal balance) => new()
    {
        Id = a.Id,
        Code = a.Code,
        Name = a.Name,
        Type = a.Type,
        IsActive = a.IsActive,
        Balance = balance
    };
}
