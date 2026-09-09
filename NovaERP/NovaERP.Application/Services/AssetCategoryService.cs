using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class AssetCategoryService : IAssetCategoryService
{
    private readonly IAssetCategoryRepository _repo;
    private readonly IValidator<CreateAssetCategoryDto> _createValidator;
    private readonly IValidator<UpdateAssetCategoryDto> _updateValidator;

    public AssetCategoryService(IAssetCategoryRepository repo,
        IValidator<CreateAssetCategoryDto> createValidator, IValidator<UpdateAssetCategoryDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<AssetCategoryDto>> GetAllAsync(int orgId)
    {
        var categories = await _repo.GetAllByOrgAsync(orgId);
        return categories.Select(ToDto).ToList();
    }

    public async Task<AssetCategoryDto> GetByIdAsync(int orgId, int id)
    {
        var category = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"AssetCategory {id} not found");
        return ToDto(category);
    }

    public async Task<AssetCategoryDto> CreateAsync(int orgId, CreateAssetCategoryDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.CodeExistsAsync(orgId, code))
            throw new InvalidOperationException($"Asset category code {code} already exists");

        var category = new AssetCategory
        {
            OrganizationId = orgId,
            Code = code,
            Name = dto.Name,
            IsActive = dto.IsActive
        };

        _repo.Add(category);
        await _repo.SaveChangesAsync();
        return ToDto(category);
    }

    public async Task<AssetCategoryDto> UpdateAsync(int orgId, int id, UpdateAssetCategoryDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var category = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"AssetCategory {id} not found");

        category.Name = dto.Name;
        category.IsActive = dto.IsActive;
        category.UpdatedDate = DateTime.UtcNow;

        _repo.Update(category);
        await _repo.SaveChangesAsync();
        return ToDto(category);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var category = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"AssetCategory {id} not found");
        _repo.Remove(category);
        await _repo.SaveChangesAsync();
    }

    private static AssetCategoryDto ToDto(AssetCategory c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Code = c.Code,
        IsActive = c.IsActive
    };
}
