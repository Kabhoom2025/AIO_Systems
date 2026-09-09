using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/bank-reconciliations")]
[Authorize]
public class BankReconciliationController : ApiControllerBase
{
    private readonly IBankReconciliationService _service;

    public BankReconciliationController(IBankReconciliationService service) => _service = service;

    [Authorize(Policy = "finance.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "finance.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "finance.view")]
    [HttpGet("match-candidates")]
    public async Task<IActionResult> GetMatchCandidates([FromQuery] int ledgerAccountId) =>
        Ok(await _service.GetMatchCandidatesAsync(OrgId, ledgerAccountId));

    [Authorize(Policy = "finance.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBankReconciliationDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "finance.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBankReconciliationDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "finance.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "finance.edit")]
    [HttpPost("{id}/lines/{lineId}/match")]
    public async Task<IActionResult> MatchLine(int id, int lineId, [FromBody] MatchBankStatementLineDto dto) =>
        Ok(await _service.MatchLineAsync(OrgId, id, lineId, dto));

    [Authorize(Policy = "finance.edit")]
    [HttpPost("{id}/lines/{lineId}/unmatch")]
    public async Task<IActionResult> UnmatchLine(int id, int lineId) =>
        Ok(await _service.UnmatchLineAsync(OrgId, id, lineId));

    [Authorize(Policy = "finance.edit")]
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id) =>
        Ok(await _service.CompleteAsync(OrgId, id));
}
