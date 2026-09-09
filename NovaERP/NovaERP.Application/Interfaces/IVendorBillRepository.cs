using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IVendorBillRepository
{
    Task<List<VendorBill>> GetAllByOrgAsync(int orgId);
    Task<VendorBill?> GetByIdAsync(int orgId, int id);
    Task<List<VendorBill>> GetByVendorIdAsync(int orgId, int vendorId);
    void Add(VendorBill bill);
    void Update(VendorBill bill);
    void Remove(VendorBill bill);
    Task SaveChangesAsync();
}
