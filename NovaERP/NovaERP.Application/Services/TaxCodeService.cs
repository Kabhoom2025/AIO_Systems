using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class TaxCodeService : ITaxCodeService
{
    private readonly ITaxCodeRepository _repo;
    private readonly IValidator<CreateTaxCodeDto> _createValidator;
    private readonly IValidator<UpdateTaxCodeDto> _updateValidator;

    public TaxCodeService(ITaxCodeRepository repo,
        IValidator<CreateTaxCodeDto> createValidator, IValidator<UpdateTaxCodeDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<TaxCodeDto>> GetAllAsync(int orgId)
    {
        var taxCodes = await _repo.GetAllByOrgAsync(orgId);
        return taxCodes.Select(ToDto).ToList();
    }

    public async Task<TaxCodeDto> GetByIdAsync(int orgId, int id)
    {
        var taxCode = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"TaxCode {id} not found");
        return ToDto(taxCode);
    }

    public async Task<TaxCodeDto> CreateAsync(int orgId, CreateTaxCodeDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.CodeExistsAsync(orgId, code))
            throw new InvalidOperationException($"Tax code {code} already exists");

        var taxCode = new TaxCode
        {
            OrganizationId = orgId,
            Code = code,
            Name = dto.Name,
            IsActive = dto.IsActive,
            Components = dto.Components.Select(c => new TaxComponent
            {
                Name = c.Name,
                RatePercent = c.RatePercent,
                DisplayOrder = c.DisplayOrder
            }).ToList()
        };

        _repo.Add(taxCode);
        await _repo.SaveChangesAsync();
        return ToDto(taxCode);
    }

    public async Task<TaxCodeDto> UpdateAsync(int orgId, int id, UpdateTaxCodeDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var taxCode = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"TaxCode {id} not found");

        taxCode.Name = dto.Name;
        taxCode.IsActive = dto.IsActive;
        taxCode.UpdatedDate = DateTime.UtcNow;

        // Components are owned by the tax code and replaced wholesale on update.
        taxCode.Components.Clear();
        foreach (var c in dto.Components)
        {
            taxCode.Components.Add(new TaxComponent
            {
                Name = c.Name,
                RatePercent = c.RatePercent,
                DisplayOrder = c.DisplayOrder
            });
        }

        _repo.Update(taxCode);
        await _repo.SaveChangesAsync();
        return ToDto(taxCode);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var taxCode = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"TaxCode {id} not found");
        _repo.Remove(taxCode);
        await _repo.SaveChangesAsync();
    }

    private static TaxCodeDto ToDto(TaxCode t) => new()
    {
        Id = t.Id,
        Code = t.Code,
        Name = t.Name,
        IsActive = t.IsActive,
        Components = t.Components.OrderBy(c => c.DisplayOrder).Select(c => new TaxComponentDto
        {
            Id = c.Id,
            Name = c.Name,
            RatePercent = c.RatePercent,
            DisplayOrder = c.DisplayOrder
        }).ToList(),
        TotalRatePercent = t.Components.Sum(c => c.RatePercent)
    };
}
