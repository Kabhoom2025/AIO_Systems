using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repo;
    private readonly IValidator<CreateAccountDto> _createValidator;
    private readonly IValidator<UpdateAccountDto> _updateValidator;

    public AccountService(IAccountRepository repo,
        IValidator<CreateAccountDto> createValidator, IValidator<UpdateAccountDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<AccountDto>> GetAllAsync(int orgId)
    {
        var accounts = await _repo.GetAllByOrgAsync(orgId);
        return accounts.Select(ToDto).ToList();
    }

    public async Task<AccountDto> GetByIdAsync(int orgId, int id)
    {
        var account = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Account {id} not found");
        return ToDto(account);
    }

    public async Task<AccountDto> CreateAsync(int orgId, CreateAccountDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var account = new Account
        {
            OrganizationId = orgId,
            Name = dto.Name,
            Industry = dto.Industry,
            Website = dto.Website,
            Phone = dto.Phone,
            OwnerId = dto.OwnerId
        };

        _repo.Add(account);
        await _repo.SaveChangesAsync();
        return ToDto(account);
    }

    public async Task<AccountDto> UpdateAsync(int orgId, int id, UpdateAccountDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var account = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Account {id} not found");

        account.Name = dto.Name;
        account.Industry = dto.Industry;
        account.Website = dto.Website;
        account.Phone = dto.Phone;
        account.OwnerId = dto.OwnerId;
        account.UpdatedDate = DateTime.UtcNow;

        _repo.Update(account);
        await _repo.SaveChangesAsync();
        return ToDto(account);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var account = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Account {id} not found");
        _repo.Remove(account);
        await _repo.SaveChangesAsync();
    }

    private static AccountDto ToDto(Account a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Industry = a.Industry,
        Website = a.Website,
        Phone = a.Phone,
        OwnerId = a.OwnerId,
        OwnerName = a.Owner?.Name ?? string.Empty
    };
}
