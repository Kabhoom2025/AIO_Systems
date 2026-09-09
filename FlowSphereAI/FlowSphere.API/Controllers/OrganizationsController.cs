using FlowSphere.Application.Organizations.Commands.SaveOrganizationSettings;
using FlowSphere.Application.Organizations.Queries.GetOrganizationSettings;
using FlowSphere.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/organizations")]
public class OrganizationsController : ApiControllerBase
{
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetOrganizationSettingsQuery(), cancellationToken);
        return FromResult(result);
    }

    public record SaveSettingsRequest(string SettingsJson);

    [HttpPut("settings")]
    [Authorize(Policy = PermissionCatalog.SettingsManage)]
    public async Task<IActionResult> SaveSettings(SaveSettingsRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveOrganizationSettingsCommand(request.SettingsJson), cancellationToken);
        return FromResult(result);
    }
}
