using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/rfqs")]
[Authorize]
public class RfqRequestController : ApiControllerBase
{
    private readonly IRfqRequestService _service;

    public RfqRequestController(IRfqRequestService service) => _service = service;

    [Authorize(Policy = "procurement.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "procurement.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "procurement.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRfqRequestDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "procurement.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRfqRequestDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "procurement.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "procurement.edit")]
    [HttpPost("{id}/send")]
    public async Task<IActionResult> Send(int id) =>
        Ok(await _service.SendAsync(OrgId, id));

    [Authorize(Policy = "procurement.edit")]
    [HttpPost("{id}/record-quote")]
    public async Task<IActionResult> RecordQuote(int id, [FromBody] RecordRfqQuoteDto dto) =>
        Ok(await _service.RecordQuoteAsync(OrgId, id, dto));

    [Authorize(Policy = "procurement.edit")]
    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] CloseRfqRequestDto dto) =>
        Ok(await _service.CloseAsync(OrgId, id, dto));

    [Authorize(Policy = "procurement.edit")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) =>
        Ok(await _service.CancelAsync(OrgId, id));
}
