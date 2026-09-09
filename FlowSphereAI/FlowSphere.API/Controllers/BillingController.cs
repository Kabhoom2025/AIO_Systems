using FlowSphere.Application.Billing.Commands.ChangePlan;
using FlowSphere.Application.Billing.Queries.GetPlans;
using FlowSphere.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/billing")]
public class BillingController : ApiControllerBase
{
    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPlansQuery(), cancellationToken);
        return FromResult(result);
    }

    public record ChangePlanRequest(string PlanName);

    [HttpPut("plan")]
    [Authorize(Policy = PermissionCatalog.BillingManage)]
    public async Task<IActionResult> ChangePlan(ChangePlanRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ChangePlanCommand(request.PlanName), cancellationToken);
        return FromResult(result);
    }
}
