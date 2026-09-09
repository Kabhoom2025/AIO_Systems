using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IBillOfMaterialService
{
    Task<List<BillOfMaterialDto>> GetAllAsync(int orgId);
    Task<BillOfMaterialDto> GetByIdAsync(int orgId, int id);
    Task<BillOfMaterialDto> CreateAsync(int orgId, CreateBillOfMaterialDto dto);
    Task<BillOfMaterialDto> UpdateAsync(int orgId, int id, UpdateBillOfMaterialDto dto);
    Task DeleteAsync(int orgId, int id);
}
