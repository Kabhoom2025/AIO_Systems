using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IVendorBillService
{
    Task<List<VendorBillDto>> GetAllAsync(int orgId);
    Task<VendorBillDto> GetByIdAsync(int orgId, int id);
    Task<VendorBillDto> CreateAsync(int orgId, CreateVendorBillDto dto);
    Task<VendorBillDto> UpdateAsync(int orgId, int id, UpdateVendorBillDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<VendorBillDto> ApproveAsync(int orgId, int id);
    Task<VendorBillDto> PayAsync(int orgId, int id, PayVendorBillDto dto);
    Task<VendorBillDto> CancelAsync(int orgId, int id);
}
