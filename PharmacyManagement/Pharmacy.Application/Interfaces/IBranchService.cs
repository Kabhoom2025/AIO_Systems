using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IBranchService
{
    Task<List<BranchDto>> GetAllAsync(int orgId);
    Task<List<PublicBranchDto>> GetPublicListAsync(int orgId);
    Task<BranchDto?> GetByIdAsync(int id);
    Task<BranchDto> CreateAsync(int orgId, CreateBranchDto dto);
    Task<BranchDto> UpdateAsync(int id, UpdateBranchDto dto);
    Task DeleteAsync(int id);
}
