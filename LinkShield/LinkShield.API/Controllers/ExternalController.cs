using System.Security.Claims;
using FluentValidation;
using LinkShield.API.Authentication;
using LinkShield.Application.DTOs.ApiClients;
using LinkShield.Application.DTOs.Scans;
using LinkShield.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkShield.API.Controllers;

/// <summary>Enterprise API surface (spec section 30) — authenticated via X-API-Key, not JWT.</summary>
[ApiController]
[Route("api/v1/external")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.SchemeName)]
public class ExternalController : ControllerBase
{
    private readonly IScanService _scanService;
    private readonly IValidator<ExternalUrlCheckRequest> _validator;

    public ExternalController(IScanService scanService, IValidator<ExternalUrlCheckRequest> validator)
    {
        _scanService = scanService;
        _validator = validator;
    }

    [HttpPost("url/check")]
    public async Task<ActionResult<ScanDetailDto>> CheckUrl(ExternalUrlCheckRequest request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);
        var apiClientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _scanService.SubmitScanAsync(request.Url, userId: null, apiClientId, ct);
        return Ok(result);
    }
}
