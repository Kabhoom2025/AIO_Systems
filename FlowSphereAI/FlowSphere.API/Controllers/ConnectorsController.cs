using FlowSphere.Application.Connectors.Commands.SetConnectorCredential;
using FlowSphere.Application.Connectors.Queries.GetConnectorsList;
using FlowSphere.Domain.Common;
using FlowSphere.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/connectors")]
public class ConnectorsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCatalog.ConnectorsManage)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetConnectorsListQuery(), cancellationToken);
        return FromResult(result);
    }

    public record SetCredentialRequest(string Value);

    [HttpPut("{connectorId:int}/credentials/{keyName}")]
    [Authorize(Policy = PermissionCatalog.ConnectorsManage)]
    public async Task<IActionResult> SetCredential(int connectorId, string keyName, SetCredentialRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SetConnectorCredentialCommand(connectorId, keyName, request.Value), cancellationToken);
        return FromResult(result);
    }
}
