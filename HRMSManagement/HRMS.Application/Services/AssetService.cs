using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _repo;

    public AssetService(IAssetRepository repo) => _repo = repo;

    public async Task<List<AssetDto>> GetAllAsync(int orgId, string? category, string? status)
    {
        var assets = await _repo.GetAllByOrgAsync(orgId, category, status);
        return assets.Select(MapToDto).ToList();
    }

    public async Task<AssetDetailDto> GetByIdAsync(int orgId, int id)
    {
        var asset = await GetOwnedAssetAsync(orgId, id);
        return MapToDetailDto(asset);
    }

    public async Task<AssetDto> CreateAsync(int orgId, CreateAssetDto dto)
    {
        var nextNumber = await _repo.GetNextTagNumberAsync(orgId);
        var asset = new Asset
        {
            OrganizationId = orgId,
            AssetTag       = $"AST-{nextNumber:D4}",
            Name           = dto.Name,
            Category       = dto.Category,
            SerialNumber   = dto.SerialNumber,
            PurchaseDate   = dto.PurchaseDate,
            PurchaseCost   = dto.PurchaseCost,
            WarrantyUntil  = dto.WarrantyUntil,
            Condition      = dto.Condition,
            Status         = "Available",
            BranchId       = dto.BranchId,
            Notes          = dto.Notes
        };
        _repo.Add(asset);
        await _repo.SaveChangesAsync();
        return MapToDto(asset);
    }

    public async Task<AssetDto> UpdateAsync(int orgId, int id, UpdateAssetDto dto)
    {
        var asset = await GetOwnedAssetAsync(orgId, id);

        asset.Name          = dto.Name;
        asset.Category      = dto.Category;
        asset.SerialNumber  = dto.SerialNumber;
        asset.PurchaseDate  = dto.PurchaseDate;
        asset.PurchaseCost  = dto.PurchaseCost;
        asset.WarrantyUntil = dto.WarrantyUntil;
        asset.Condition     = dto.Condition;
        asset.Status        = dto.Status;
        asset.BranchId      = dto.BranchId;
        asset.Notes         = dto.Notes;
        asset.UpdatedDate   = DateTime.UtcNow;

        _repo.Update(asset);
        await _repo.SaveChangesAsync();
        return MapToDto(asset);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var asset = await GetOwnedAssetAsync(orgId, id);
        if (asset.Allocations.Count > 0)
            throw new InvalidOperationException("Cannot delete an asset that has allocation history.");

        _repo.Remove(asset);
        await _repo.SaveChangesAsync();
    }

    public async Task<AssetDto> AllocateAsync(int orgId, int id, int allocatedByUserId, AllocateAssetDto dto)
    {
        var asset = await GetOwnedAssetAsync(orgId, id);
        if (asset.Status != "Available")
            throw new InvalidOperationException("Asset is not available.");

        var allocation = new AssetAllocation
        {
            AssetId           = asset.Id,
            EmployeeId        = dto.EmployeeId,
            AllocatedDate     = DateOnly.FromDateTime(DateTime.UtcNow),
            AllocatedByUserId = allocatedByUserId,
            Notes             = dto.Notes
        };
        _repo.AddAllocation(allocation);

        asset.Status      = "Allocated";
        asset.UpdatedDate = DateTime.UtcNow;
        _repo.Update(asset);
        await _repo.SaveChangesAsync();

        var refreshed = await _repo.GetByIdAsync(asset.Id) ?? asset;
        return MapToDto(refreshed);
    }

    public async Task<AssetDto> ReturnAsync(int orgId, int id, ReturnAssetDto dto)
    {
        var asset = await GetOwnedAssetAsync(orgId, id);
        var open = asset.Allocations.FirstOrDefault(al => al.ReturnedDate == null)
            ?? throw new InvalidOperationException("Asset has no active allocation to return.");

        open.ReturnedDate    = DateOnly.FromDateTime(DateTime.UtcNow);
        open.ReturnCondition = dto.ReturnCondition;
        open.Notes           = dto.Notes ?? open.Notes;

        asset.Status = "Available";
        if (!string.IsNullOrWhiteSpace(dto.ReturnCondition))
            asset.Condition = dto.ReturnCondition;
        asset.UpdatedDate = DateTime.UtcNow;

        _repo.Update(asset);
        await _repo.SaveChangesAsync();
        return MapToDto(asset);
    }

    public async Task<List<AssetDto>> GetMyAssetsAsync(int orgId, int employeeId)
    {
        var assets = await _repo.GetAllocatedToEmployeeAsync(orgId, employeeId);
        return assets.Select(MapToDto).ToList();
    }

    private async Task<Asset> GetOwnedAssetAsync(int orgId, int id)
    {
        var asset = await _repo.GetByIdAsync(id);
        if (asset == null || asset.OrganizationId != orgId)
            throw new KeyNotFoundException($"Asset {id} not found");
        return asset;
    }

    private static AssetDto MapToDto(Asset a)
    {
        var dto = new AssetDto();
        CopyAssetFields(dto, a);
        return dto;
    }

    private static AssetDetailDto MapToDetailDto(Asset a)
    {
        var dto = new AssetDetailDto();
        CopyAssetFields(dto, a);
        dto.Allocations = a.Allocations
            .OrderByDescending(al => al.AllocatedDate)
            .Select(MapAllocation)
            .ToList();
        return dto;
    }

    private static void CopyAssetFields(AssetDto dto, Asset a)
    {
        var open = a.Allocations.FirstOrDefault(al => al.ReturnedDate == null);

        dto.Id                      = a.Id;
        dto.AssetTag                = a.AssetTag;
        dto.Name                    = a.Name;
        dto.Category                = a.Category;
        dto.SerialNumber            = a.SerialNumber;
        dto.PurchaseDate            = a.PurchaseDate;
        dto.PurchaseCost            = a.PurchaseCost;
        dto.WarrantyUntil           = a.WarrantyUntil;
        dto.Condition               = a.Condition;
        dto.Status                  = a.Status;
        dto.Notes                   = a.Notes;
        dto.BranchId                = a.BranchId;
        dto.BranchName              = a.Branch?.Name;
        dto.CurrentHolderEmployeeId = open?.EmployeeId;
        dto.CurrentHolderName       = open?.Employee?.FullName;
    }

    private static AllocationDto MapAllocation(AssetAllocation al) => new()
    {
        Id                = al.Id,
        EmployeeId        = al.EmployeeId,
        EmployeeName      = al.Employee?.FullName ?? string.Empty,
        AllocatedDate     = al.AllocatedDate,
        ReturnedDate      = al.ReturnedDate,
        ReturnCondition   = al.ReturnCondition,
        Notes             = al.Notes,
        AllocatedByUserId = al.AllocatedByUserId,
        AllocatedByName   = al.AllocatedByUser?.Name
    };
}
