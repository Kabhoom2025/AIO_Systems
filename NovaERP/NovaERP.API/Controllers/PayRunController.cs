using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/pay-runs")]
[Authorize]
public class PayRunController : ApiControllerBase
{
    private readonly IPayRunService _service;

    public PayRunController(IPayRunService service) => _service = service;

    [Authorize(Policy = "payroll.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "payroll.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "payroll.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePayRunDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "payroll.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePayRunDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "payroll.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "payroll.edit")]
    [HttpPost("{id}/process")]
    public async Task<IActionResult> Process(int id) =>
        Ok(await _service.ProcessAsync(OrgId, id));

    [Authorize(Policy = "payroll.edit")]
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> Pay(int id, [FromBody] PayPayRunDto dto) =>
        Ok(await _service.PayAsync(OrgId, id, dto));

    [Authorize(Policy = "payroll.edit")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) =>
        Ok(await _service.CancelAsync(OrgId, id));
}
