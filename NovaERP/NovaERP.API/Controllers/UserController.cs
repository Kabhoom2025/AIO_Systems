using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/users")]
[Authorize]
public class UserController : ApiControllerBase
{
    private readonly IUserService _service;
    private readonly ISalesOrderService _salesOrderService;

    public UserController(IUserService service, ISalesOrderService salesOrderService)
    {
        _service = service;
        _salesOrderService = salesOrderService;
    }

    [Authorize(Policy = "users.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "users.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(OrgId, id);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "users.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "users.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "users.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "users.edit")]
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetUserPasswordDto dto)
    {
        await _service.ResetPasswordAsync(OrgId, id, dto);
        return NoContent();
    }

    /// <summary>Gated by sales.view (not users.view) since this exposes sales data, not just
    /// profile data — a caller could have one permission without the other.</summary>
    [Authorize(Policy = "sales.view")]
    [HttpGet("{id}/sales-summary")]
    public async Task<IActionResult> GetSalesSummary(int id) =>
        Ok(await _salesOrderService.GetSummaryForOwnerAsync(OrgId, id));
}
