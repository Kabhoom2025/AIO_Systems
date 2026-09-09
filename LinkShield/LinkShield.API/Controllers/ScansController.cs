using System.Security.Claims;
using FluentValidation;
using LinkShield.Application.DTOs.Scans;
using LinkShield.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkShield.API.Controllers;

[ApiController]
[Route("api/v1/scans")]
public class ScansController : ControllerBase
{
    private readonly IScanService _scanService;
    private readonly IValidator<AnalyzeUrlRequestDto> _validator;

    public ScansController(IScanService scanService, IValidator<AnalyzeUrlRequestDto> validator)
    {
        _scanService = scanService;
        _validator = validator;
    }

    /// <summary>Creates a scan and runs whatever stages are currently implemented (today:
    /// URL analysis only — see ScanService). Returns the full result directly since the
    /// pipeline is still fast/synchronous at this phase; once later stages add real network
    /// calls this will move to 202 + background processing (LinkShield.Worker) with
    /// ScanProgressHub broadcasting progress.</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ScanDetailDto>> Submit(AnalyzeUrlRequestDto request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);
        var userId = TryGetUserId();
        var result = await _scanService.SubmitScanAsync(request.Url, userId, apiClientId: null, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.ScanId }, result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ScanDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _scanService.GetScanAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResultDto<ScanSummaryDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _scanService.GetScansAsync(page, pageSize, userId: null, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _scanService.DeleteScanAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    private Guid? TryGetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var userId) ? userId : null;
    }
}
