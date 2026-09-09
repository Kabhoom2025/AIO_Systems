using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

/// <summary>Self-service endpoints scoped to the caller's own linked Vendor record — no
/// procurement.view/purchase.view policy on any action here (see IVendorPortalService's doc
/// comment for why). Just [Authorize]: any logged-in user, regardless of role/permissions,
/// can reach their own data.</summary>
[Route("api/vendor-portal")]
[Authorize]
public class VendorPortalController : ApiControllerBase
{
    private readonly IVendorPortalService _service;

    public VendorPortalController(IVendorPortalService service) => _service = service;

    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile() =>
        Ok(await _service.GetMyProfileAsync(OrgId, UserId));

    [HttpGet("purchase-orders")]
    public async Task<IActionResult> GetMyPurchaseOrders() =>
        Ok(await _service.GetMyPurchaseOrdersAsync(OrgId, UserId));

    [HttpGet("bills")]
    public async Task<IActionResult> GetMyBills() =>
        Ok(await _service.GetMyBillsAsync(OrgId, UserId));
}
