using FluentValidation;
using LinkShield.Application.DTOs.Scans;
using LinkShield.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LinkShield.API.Controllers;

[ApiController]
[Route("api/v1/url")]
public class UrlController : ControllerBase
{
    private readonly IUrlAnalyzer _urlAnalyzer;
    private readonly IValidator<AnalyzeUrlRequestDto> _validator;

    public UrlController(IUrlAnalyzer urlAnalyzer, IValidator<AnalyzeUrlRequestDto> validator)
    {
        _urlAnalyzer = urlAnalyzer;
        _validator = validator;
    }

    /// <summary>URL-analysis stage only, synchronous — no persistence, no other stages
    /// (domain/DNS/SSL/threat-intel/etc). For a full scan use POST /api/v1/scans.</summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<UrlAnalysisResultDto>> Analyze(AnalyzeUrlRequestDto request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);
        return Ok(_urlAnalyzer.Analyze(request.Url));
    }
}
