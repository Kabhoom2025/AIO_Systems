using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCurrencyDto> _createValidator;
    private readonly IValidator<UpdateCurrencyDto> _updateValidator;

    public CurrencyService(ICurrencyRepository repo, IMapper mapper,
        IValidator<CreateCurrencyDto> createValidator, IValidator<UpdateCurrencyDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<CurrencyDto>> GetAllAsync()
    {
        var currencies = await _repo.GetAllAsync();
        return currencies.Select(_mapper.Map<CurrencyDto>).ToList();
    }

    public async Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.GetByCodeAsync(code) != null)
            throw new InvalidOperationException($"Currency {code} already exists");

        var currency = _mapper.Map<Currency>(dto);
        currency.Code = code;
        currency.IsActive = true;

        _repo.Add(currency);
        await _repo.SaveChangesAsync();
        return _mapper.Map<CurrencyDto>(currency);
    }

    public async Task<CurrencyDto> UpdateAsync(string code, UpdateCurrencyDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var currency = await _repo.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException($"Currency {code} not found");

        _mapper.Map(dto, currency);
        currency.UpdatedDate = DateTime.UtcNow;

        _repo.Update(currency);
        await _repo.SaveChangesAsync();
        return _mapper.Map<CurrencyDto>(currency);
    }
}
