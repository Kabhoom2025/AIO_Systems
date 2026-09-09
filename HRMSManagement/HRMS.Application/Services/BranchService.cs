using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class BranchService : IBranchService
{
    private readonly IBranchRepository _repo;

    public BranchService(IBranchRepository repo) => _repo = repo;

    public async Task<List<BranchDto>> GetAllAsync(int orgId)
    {
        var branches = await _repo.GetAllByOrgAsync(orgId);
        return branches.Select(MapToDto).ToList();
    }

    public async Task<BranchDto> GetByIdAsync(int orgId, int id)
    {
        var branch = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Branch {id} not found");
        return MapToDto(branch);
    }

    public async Task<BranchDto> CreateAsync(int orgId, CreateBranchDto dto)
    {
        var branch = new Branch
        {
            OrganizationId = orgId,
            Name           = dto.Name,
            Code           = dto.Code,
            Address        = dto.Address,
            City           = dto.City,
            State          = dto.State,
            Country        = dto.Country,
            Phone          = dto.Phone,
            Email          = dto.Email,
            Timezone       = dto.Timezone,
            IsHeadOffice   = dto.IsHeadOffice,
            IsActive       = true
        };
        _repo.Add(branch);
        await _repo.SaveChangesAsync();
        return MapToDto(branch);
    }

    public async Task<BranchDto> UpdateAsync(int orgId, int id, UpdateBranchDto dto)
    {
        var branch = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Branch {id} not found");

        branch.Name         = dto.Name;
        branch.Code         = dto.Code;
        branch.Address      = dto.Address;
        branch.City         = dto.City;
        branch.State        = dto.State;
        branch.Country      = dto.Country;
        branch.Phone        = dto.Phone;
        branch.Email        = dto.Email;
        branch.Timezone     = dto.Timezone;
        branch.IsHeadOffice = dto.IsHeadOffice;
        branch.IsActive     = dto.IsActive;
        branch.UpdatedDate  = DateTime.UtcNow;

        _repo.Update(branch);
        await _repo.SaveChangesAsync();
        return MapToDto(branch);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var branch = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Branch {id} not found");
        _repo.Remove(branch);
        await _repo.SaveChangesAsync();
    }

    private static BranchDto MapToDto(Branch b) => new()
    {
        Id           = b.Id,
        Name         = b.Name,
        Code         = b.Code,
        Address      = b.Address,
        City         = b.City,
        State        = b.State,
        Country      = b.Country,
        Phone        = b.Phone,
        Email        = b.Email,
        Timezone     = b.Timezone,
        IsHeadOffice = b.IsHeadOffice,
        IsActive     = b.IsActive
    };
}
