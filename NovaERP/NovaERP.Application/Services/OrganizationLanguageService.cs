using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class OrganizationLanguageService : IOrganizationLanguageService
{
    private readonly IOrganizationLanguageRepository _repo;
    private readonly ILanguageRepository _languageRepo;
    private readonly IValidator<UpdateOrganizationLanguagesDto> _updateValidator;

    public OrganizationLanguageService(IOrganizationLanguageRepository repo, ILanguageRepository languageRepo,
        IValidator<UpdateOrganizationLanguagesDto> updateValidator)
    {
        _repo = repo;
        _languageRepo = languageRepo;
        _updateValidator = updateValidator;
    }

    public async Task<List<OrganizationLanguageDto>> GetAllAsync(int orgId)
    {
        var items = await _repo.GetAllByOrgAsync(orgId);
        return await ToDtosAsync(items);
    }

    public async Task<List<OrganizationLanguageDto>> UpdateAsync(int orgId, UpdateOrganizationLanguagesDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var existing = await _repo.GetAllByOrgAsync(orgId);
        _repo.RemoveRange(existing);

        var updated = dto.LanguageCodes
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(code => new OrganizationLanguage
            {
                OrganizationId = orgId,
                LanguageCode = code,
                IsDefault = string.Equals(code, dto.DefaultLanguageCode, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        _repo.AddRange(updated);
        await _repo.SaveChangesAsync();
        return await ToDtosAsync(updated);
    }

    private async Task<List<OrganizationLanguageDto>> ToDtosAsync(List<OrganizationLanguage> items)
    {
        var languages = await _languageRepo.GetAllActiveAsync();
        var nameByCode = languages.ToDictionary(l => l.Code, l => l.Name, StringComparer.OrdinalIgnoreCase);

        return items.Select(i => new OrganizationLanguageDto
        {
            Id = i.Id,
            LanguageCode = i.LanguageCode,
            LanguageName = nameByCode.TryGetValue(i.LanguageCode, out var name) ? name : i.LanguageCode,
            IsDefault = i.IsDefault
        }).ToList();
    }
}
