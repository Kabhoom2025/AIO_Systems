using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/vendor-bills")]
[Authorize]
public class VendorBillController : ApiControllerBase
{
    private readonly IVendorBillService _service;

    public VendorBillController(IVendorBillService service) => _service = service;

    [Authorize(Policy = "finance.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "finance.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "finance.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorBillDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "finance.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVendorBillDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "finance.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "finance.edit")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id) =>
        Ok(await _service.ApproveAsync(OrgId, id));

    [Authorize(Policy = "finance.edit")]
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> Pay(int id, [FromBody] PayVendorBillDto dto) =>
        Ok(await _service.PayAsync(OrgId, id, dto));

    [Authorize(Policy = "finance.edit")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) =>
        Ok(await _service.CancelAsync(OrgId, id));
}
