using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IValidator<CreateAssetDto> _createValidator;
    private readonly IValidator<UpdateAssetDto> _updateValidator;
    private readonly IValidator<AssignAssetDto> _assignValidator;

    public AssetService(IAssetRepository repo, IEmployeeRepository employeeRepo,
        IValidator<CreateAssetDto> createValidator, IValidator<UpdateAssetDto> updateValidator,
        IValidator<AssignAssetDto> assignValidator)
    {
        _repo = repo;
        _employeeRepo = employeeRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _assignValidator = assignValidator;
    }

    public async Task<List<AssetDto>> GetAllAsync(int orgId)
    {
        var assets = await _repo.GetAllByOrgAsync(orgId);
        return assets.Select(ToDto).ToList();
    }

    public async Task<AssetDto> GetByIdAsync(int orgId, int id)
    {
        var asset = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Asset {id} not found");
        return ToDto(asset);
    }

    public async Task<AssetDto> CreateAsync(int orgId, CreateAssetDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var asset = new Asset
        {
            OrganizationId = orgId,
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            SerialNumber = dto.SerialNumber,
            PurchaseDate = dto.PurchaseDate,
            PurchaseCost = dto.PurchaseCost,
            WarrantyExpiryDate = dto.WarrantyExpiryDate,
            Status = "Available"
        };

        _repo.Add(asset);
        await _repo.SaveChangesAsync();

        // AssetCode depends on the generated Id, so it's set in a second save — same scheme
        // as SalesOrder.OrderNumber/Employee.EmployeeCode.
        asset.AssetCode = $"AST-{asset.Id:D5}";
        _repo.Update(asset);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, asset.Id) ?? asset;
        return ToDto(reloaded);
    }

    public async Task<AssetDto> UpdateAsync(int orgId, int id, UpdateAssetDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var asset = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Asset {id} not found");

        if (asset.Status == "Retired")
            throw new InvalidOperationException("Retired assets cannot be edited.");

        asset.Name = dto.Name;
        asset.CategoryId = dto.CategoryId;
        asset.SerialNumber = dto.SerialNumber;
        asset.PurchaseDate = dto.PurchaseDate;
        asset.PurchaseCost = dto.PurchaseCost;
        asset.WarrantyExpiryDate = dto.WarrantyExpiryDate;
        asset.UpdatedDate = DateTime.UtcNow;

        _repo.Update(asset);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, asset.Id) ?? asset;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var asset = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Asset {id} not found");

        if (asset.Status == "Retired")
            throw new InvalidOperationException("Retired assets cannot be deleted.");

        _repo.Remove(asset);
        await _repo.SaveChangesAsync();
    }

    public async Task<AssetDto> AssignAsync(int orgId, int id, AssignAssetDto dto)
    {
        await _assignValidator.ValidateAndThrowAsync(dto);

        var asset = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Asset {id} not found");

        if (asset.Status != "Available")
            throw new InvalidOperationException("Only available assets can be assigned.");

        _ = await _employeeRepo.GetByIdAsync(orgId, dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee {dto.EmployeeId} not found");

        asset.AssignedToId = dto.EmployeeId;
        asset.Status = "Assigned";
        asset.UpdatedDate = DateTime.UtcNow;
        _repo.Update(asset);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, asset.Id) ?? asset;
        return ToDto(reloaded);
    }

    public async Task<AssetDto> UnassignAsync(int orgId, int id)
    {
        var asset = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Asset {id} not found");

        if (asset.Status != "Assigned")
            throw new InvalidOperationException("Only assigned assets can be unassigned.");

        asset.AssignedToId = null;
        asset.Status = "Available";
        asset.UpdatedDate = DateTime.UtcNow;
        _repo.Update(asset);
        await _repo.SaveChangesAsync();

        return ToDto(asset);
    }

    public async Task<AssetDto> RetireAsync(int orgId, int id)
    {
        var asset = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Asset {id} not found");

        if (asset.Status == "Retired")
            throw new InvalidOperationException("Asset is already retired.");

        asset.Status = "Retired";
        asset.UpdatedDate = DateTime.UtcNow;
        _repo.Update(asset);
        await _repo.SaveChangesAsync();

        return ToDto(asset);
    }

    private static AssetDto ToDto(Asset a) => new()
    {
        Id = a.Id,
        AssetCode = a.AssetCode,
        Name = a.Name,
        CategoryId = a.CategoryId,
        CategoryName = a.Category?.Name ?? string.Empty,
        SerialNumber = a.SerialNumber,
        PurchaseDate = a.PurchaseDate,
        PurchaseCost = a.PurchaseCost,
        WarrantyExpiryDate = a.WarrantyExpiryDate,
        AssignedToId = a.AssignedToId,
        AssignedToName = a.AssignedTo != null ? $"{a.AssignedTo.FirstName} {a.AssignedTo.LastName}" : null,
        Status = a.Status
    };
}
