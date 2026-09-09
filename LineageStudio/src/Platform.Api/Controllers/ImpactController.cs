using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.ImpactAnalysis;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/impact")]
public class ImpactController : ControllerBase
{
    private readonly IImpactAnalysisService _impact;

    public ImpactController(IImpactAnalysisService impact)
    {
        _impact = impact;
    }

    [HttpGet("table/{tableId:guid}")]
    public async Task<ActionResult<TableImpactDto>> GetTableImpact(Guid tableId, CancellationToken ct)
        => Ok(await _impact.GetTableImpactAsync(tableId, ct));

    [HttpGet("column/{columnId:guid}")]
    public async Task<ActionResult<ColumnImpactDto>> GetColumnImpact(Guid columnId, CancellationToken ct)
        => Ok(await _impact.GetColumnImpactAsync(columnId, ct));
}
