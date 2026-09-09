using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateExchangeRateDto> _createValidator;
    private readonly IValidator<UpdateExchangeRateDto> _updateValidator;

    public ExchangeRateService(IExchangeRateRepository repo, IMapper mapper,
        IValidator<CreateExchangeRateDto> createValidator, IValidator<UpdateExchangeRateDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<ExchangeRateDto>> GetAllAsync(int orgId)
    {
        var rates = await _repo.GetAllByOrgAsync(orgId);
        return rates.Select(_mapper.Map<ExchangeRateDto>).ToList();
    }

    public async Task<ExchangeRateDto> GetByIdAsync(int orgId, int id)
    {
        var rate = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Exchange rate {id} not found");
        return _mapper.Map<ExchangeRateDto>(rate);
    }

    public async Task<ExchangeRateDto> CreateAsync(int orgId, CreateExchangeRateDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var rate = _mapper.Map<ExchangeRate>(dto);
        rate.OrganizationId = orgId;
        rate.FromCurrencyCode = dto.FromCurrencyCode.ToUpperInvariant();
        rate.ToCurrencyCode = dto.ToCurrencyCode.ToUpperInvariant();

        _repo.Add(rate);
        await _repo.SaveChangesAsync();
        return _mapper.Map<ExchangeRateDto>(rate);
    }

    public async Task<ExchangeRateDto> UpdateAsync(int orgId, int id, UpdateExchangeRateDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var rate = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Exchange rate {id} not found");

        _mapper.Map(dto, rate);
        rate.UpdatedDate = DateTime.UtcNow;

        _repo.Update(rate);
        await _repo.SaveChangesAsync();
        return _mapper.Map<ExchangeRateDto>(rate);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var rate = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Exchange rate {id} not found");
        _repo.Remove(rate);
        await _repo.SaveChangesAsync();
    }
}
