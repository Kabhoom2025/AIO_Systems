using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class BranchService : IBranchService
{
    private readonly IBranchRepository _repo;

    public BranchService(IBranchRepository repo) => _repo = repo;

    public async Task<List<BranchDto>> GetAllAsync(int orgId)
    {
        var branches = await _repo.GetAllByOrgAsync(orgId);
        return branches.Select(MapToDto).ToList();
    }

    public async Task<List<PublicBranchDto>> GetPublicListAsync(int orgId)
    {
        var branches = await _repo.GetAllByOrgAsync(orgId);
        return branches
            .Where(b => b.IsActive && b.Type != "Warehouse")
            .Select(b => new PublicBranchDto
            {
                Id      = b.Id,
                Name    = b.Name,
                Address = b.Address,
                Phone   = b.Phone,
                Email   = b.Email
            })
            .ToList();
    }

    public async Task<BranchDto?> GetByIdAsync(int id)
    {
        var branch = await _repo.GetByIdAsync(id);
        return branch == null ? null : MapToDto(branch);
    }

    public async Task<BranchDto> CreateAsync(int orgId, CreateBranchDto dto)
    {
        var branch = new Branch
        {
            OrganizationId = orgId,
            Name           = dto.Name,
            Code           = dto.Code,
            Type           = dto.Type,
            Address        = dto.Address,
            Phone          = dto.Phone,
            Email          = dto.Email,
            IsActive       = true
        };
        _repo.Add(branch);
        await _repo.SaveChangesAsync();
        return MapToDto(branch);
    }

    public async Task<BranchDto> UpdateAsync(int id, UpdateBranchDto dto)
    {
        var branch = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Branch {id} not found");

        branch.Name        = dto.Name;
        branch.Code        = dto.Code;
        branch.Type        = dto.Type;
        branch.Address     = dto.Address;
        branch.Phone       = dto.Phone;
        branch.Email       = dto.Email;
        branch.IsActive    = dto.IsActive;
        branch.UpdatedDate = DateTime.UtcNow;

        _repo.Update(branch);
        await _repo.SaveChangesAsync();
        return MapToDto(branch);
    }

    public async Task DeleteAsync(int id)
    {
        var branch = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Branch {id} not found");
        _repo.Remove(branch);
        await _repo.SaveChangesAsync();
    }

    private static BranchDto MapToDto(Branch b) => new()
    {
        Id       = b.Id,
        Name     = b.Name,
        Code     = b.Code,
        Type     = b.Type,
        Address  = b.Address,
        Phone    = b.Phone,
        Email    = b.Email,
        IsActive = b.IsActive
    };
}
