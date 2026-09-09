using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IVendorPortalService
{
    Task<MyVendorDto> GetMyProfileAsync(int orgId, int userId);
    Task<List<PurchaseOrderDto>> GetMyPurchaseOrdersAsync(int orgId, int userId);
    Task<List<VendorBillDto>> GetMyBillsAsync(int orgId, int userId);
}
