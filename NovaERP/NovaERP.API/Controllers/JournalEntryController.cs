using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/journal-entries")]
[Authorize]
public class JournalEntryController : ApiControllerBase
{
    private readonly IJournalEntryService _service;

    public JournalEntryController(IJournalEntryService service) => _service = service;

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
    public async Task<IActionResult> Create([FromBody] CreateJournalEntryDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "finance.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJournalEntryDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "finance.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "finance.edit")]
    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id) =>
        Ok(await _service.PostAsync(OrgId, id));

    [Authorize(Policy = "finance.edit")]
    [HttpPost("{id}/void")]
    public async Task<IActionResult> Void(int id) =>
        Ok(await _service.VoidAsync(OrgId, id));
}
