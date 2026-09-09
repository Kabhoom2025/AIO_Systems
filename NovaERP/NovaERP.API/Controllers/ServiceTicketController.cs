using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/service-tickets")]
[Authorize]
public class ServiceTicketController : ApiControllerBase
{
    private readonly IServiceTicketService _service;

    public ServiceTicketController(IServiceTicketService service) => _service = service;

    [Authorize(Policy = "service-desk.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "service-desk.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "service-desk.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceTicketDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "service-desk.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceTicketDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "service-desk.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "service-desk.edit")]
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignTicketDto dto) =>
        Ok(await _service.AssignAsync(OrgId, id, dto));

    [Authorize(Policy = "service-desk.edit")]
    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolveTicketDto dto) =>
        Ok(await _service.ResolveAsync(OrgId, id, dto));

    [Authorize(Policy = "service-desk.edit")]
    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(int id) =>
        Ok(await _service.CloseAsync(OrgId, id));

    [Authorize(Policy = "service-desk.edit")]
    [HttpPost("{id}/reopen")]
    public async Task<IActionResult> Reopen(int id) =>
        Ok(await _service.ReopenAsync(OrgId, id));
}
