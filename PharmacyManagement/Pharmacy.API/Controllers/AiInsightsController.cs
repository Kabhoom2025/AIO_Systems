using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/ai-insights")]
[Authorize]
public class AiInsightsController : ControllerBase
{
    private readonly IAiInsightsService _service;

    public AiInsightsController(IAiInsightsService service) => _service = service;

    [Authorize(Policy = "reports.view")]
    [HttpGet("reorder-suggestions/org/{orgId}")]
    public async Task<IActionResult> ReorderSuggestions(int orgId) =>
        Ok(await _service.GetReorderSuggestionsAsync(orgId));

    [Authorize(Policy = "reports.view")]
    [HttpGet("expiry-risk/org/{orgId}")]
    public async Task<IActionResult> ExpiryRisk(int orgId) =>
        Ok(await _service.GetExpiryRiskAsync(orgId));
}
