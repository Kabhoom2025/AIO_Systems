using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

/// <summary>Self-service endpoints scoped to the caller's own linked Contact record — no
/// crm.view policy on any action here (see ICustomerPortalService's doc comment for why).
/// Just [Authorize]: any logged-in user, regardless of role/permissions, can reach their own
/// data.</summary>
[Route("api/my-account")]
[Authorize]
public class CustomerPortalController : ApiControllerBase
{
    private readonly ICustomerPortalService _service;

    public CustomerPortalController(ICustomerPortalService service) => _service = service;

    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile() =>
        Ok(await _service.GetMyProfileAsync(OrgId, UserId));

    [HttpGet("orders")]
    public async Task<IActionResult> GetMyOrders() =>
        Ok(await _service.GetMyOrdersAsync(OrgId, UserId));

    [HttpGet("invoices")]
    public async Task<IActionResult> GetMyInvoices() =>
        Ok(await _service.GetMyInvoicesAsync(OrgId, UserId));
}
