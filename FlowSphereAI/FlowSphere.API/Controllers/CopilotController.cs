using FlowSphere.Application.Copilot.Commands.GenerateApp;
using FlowSphere.Application.Copilot.Commands.GenerateWorkflowGraph;
using FlowSphere.Domain.Common;
using FlowSphere.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/copilot")]
public class CopilotController : ApiControllerBase
{
    public record GenerateWorkflowRequest(string Prompt, string? CurrentGraphJson = null);

    [HttpPost("generate-workflow")]
    [Authorize(Policy = PermissionCatalog.WorkflowsWrite)]
    public async Task<IActionResult> GenerateWorkflow(GenerateWorkflowRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GenerateWorkflowGraphCommand(request.Prompt, request.CurrentGraphJson), cancellationToken);
        return FromResult(result);
    }

    public record GenerateAppRequest(string Prompt);

    [HttpPost("generate-app")]
    [Authorize(Policy = PermissionCatalog.AppsWrite)]
    public async Task<IActionResult> GenerateApp(GenerateAppRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GenerateAppCommand(request.Prompt), cancellationToken);
        return FromResult(result);
    }
}
