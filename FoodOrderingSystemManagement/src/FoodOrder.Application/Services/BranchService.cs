using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs.Branch;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class BranchService(IBranchRepository branchRepository, ICurrentUserContext currentUser) : IBranchService
{
    public async Task<IReadOnlyList<BranchDto>> GetMineAsync(int? explicitOrganizationId)
    {
        var organizationId = currentUser.OrganizationId
            ?? explicitOrganizationId
            ?? throw new AppException("organizationId is required for SuperAdmin.", 400);

        var branches = await branchRepository.GetByOrganizationAsync(organizationId);
        return branches.Select(ToDto).ToList();
    }

    public async Task<BranchDto> CreateAsync(CreateBranchDto dto)
    {
        var organizationId = currentUser.OrganizationId
            ?? dto.OrganizationId
            ?? throw new AppException("organizationId is required for SuperAdmin.", 400);

        var branch = new Branch
        {
            OrganizationId = organizationId,
            Name = dto.Name.Trim(),
            Address = dto.Address?.Trim(),
            Phone = dto.Phone?.Trim(),
            IsActive = true,
        };

        var created = await branchRepository.CreateAsync(branch);
        return ToDto(created);
    }

    public async Task<BranchDto> UpdateAsync(int id, UpdateBranchDto dto)
    {
        var branch = await branchRepository.GetByIdAsync(id)
            ?? throw new AppException("Branch not found.", 404);

        if (currentUser.OrganizationId.HasValue && branch.OrganizationId != currentUser.OrganizationId)
            throw new AppException("Branch not found.", 404);

        branch.Name = dto.Name.Trim();
        branch.Address = dto.Address?.Trim();
        branch.Phone = dto.Phone?.Trim();
        branch.IsActive = dto.IsActive;

        await branchRepository.UpdateAsync(branch);
        return ToDto(branch);
    }

    public async Task DeleteAsync(int id)
    {
        var branch = await branchRepository.GetByIdAsync(id)
            ?? throw new AppException("Branch not found.", 404);

        if (currentUser.OrganizationId.HasValue && branch.OrganizationId != currentUser.OrganizationId)
            throw new AppException("Branch not found.", 404);

        if (branch.IsDefault)
            throw new AppException("The default branch cannot be deleted.", 400);

        await branchRepository.DeleteAsync(id);
    }

    public async Task<List<BranchReportDto>> GetBranchReportsAsync(int? explicitOrganizationId)
    {
        var organizationId = currentUser.OrganizationId
            ?? explicitOrganizationId
            ?? throw new AppException("organizationId is required for SuperAdmin.", 400);

        return await branchRepository.GetBranchReportsAsync(organizationId);
    }

    private static BranchDto ToDto(Branch b) => new()
    {
        Id = b.Id,
        OrganizationId = b.OrganizationId,
        Name = b.Name,
        Address = b.Address,
        Phone = b.Phone,
        IsActive = b.IsActive,
        IsDefault = b.IsDefault,
        CreatedDate = b.CreatedDate,
    };
}
