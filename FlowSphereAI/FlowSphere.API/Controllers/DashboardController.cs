using FlowSphere.Application.Dashboard.Queries.GetDashboardKpis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/dashboard")]
public class DashboardController : ApiControllerBase
{
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDashboardKpisQuery(), cancellationToken);
        return FromResult(result);
    }
}
