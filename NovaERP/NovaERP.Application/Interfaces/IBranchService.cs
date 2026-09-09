using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IBranchService
{
    Task<List<BranchDto>> GetAllAsync(int orgId);
    Task<BranchDto> GetByIdAsync(int orgId, int id);
    Task<BranchDto> CreateAsync(int orgId, CreateBranchDto dto);
    Task<BranchDto> UpdateAsync(int orgId, int id, UpdateBranchDto dto);
    Task DeleteAsync(int orgId, int id);
}
