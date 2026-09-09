using FlowSphere.Application.Executions.Queries.GetExecutionHistory;
using FlowSphere.Application.Workflows.Commands.CreateWorkflow;
using FlowSphere.Application.Workflows.Commands.CreateWorkflowVersion;
using FlowSphere.Application.Workflows.Commands.ExecuteWorkflow;
using FlowSphere.Application.Workflows.Commands.PromoteWorkflowVersion;
using FlowSphere.Application.Workflows.Commands.PublishWorkflowVersion;
using FlowSphere.Application.Workflows.Queries.GetWorkflowById;
using FlowSphere.Application.Workflows.Queries.GetWorkflowsList;
using FlowSphere.Application.Workflows.Queries.GetWorkflowVersionById;
using FlowSphere.Application.Workflows.Queries.GetWorkflowVersionsList;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;
using FlowSphere.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/workflows")]
public class WorkflowsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCatalog.WorkflowsRead)]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetWorkflowsListQuery(page, pageSize), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionCatalog.WorkflowsRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetWorkflowByIdQuery(id), cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.WorkflowsWrite)]
    public async Task<IActionResult> Create(CreateWorkflowCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, id => CreatedAtAction(nameof(GetById), new { id }, new { id }));
    }

    public record SaveGraphRequest(string GraphJson);

    [HttpPost("{id:int}/versions")]
    [Authorize(Policy = PermissionCatalog.WorkflowsWrite)]
    public async Task<IActionResult> CreateVersion(int id, SaveGraphRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new CreateWorkflowVersionCommand(id, request.GraphJson), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:int}/versions")]
    [Authorize(Policy = PermissionCatalog.WorkflowsRead)]
    public async Task<IActionResult> GetVersions(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetWorkflowVersionsListQuery(id), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:int}/versions/{versionId:int}")]
    [Authorize(Policy = PermissionCatalog.WorkflowsRead)]
    public async Task<IActionResult> GetVersion(int id, int versionId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetWorkflowVersionByIdQuery(id, versionId), cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{id:int}/versions/{versionId:int}/publish")]
    [Authorize(Policy = PermissionCatalog.WorkflowsPublish)]
    public async Task<IActionResult> PublishVersion(int id, int versionId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new PublishWorkflowVersionCommand(id, versionId), cancellationToken);
        return FromResult(result);
    }

    public record PromoteVersionRequest(EnvironmentStage TargetStage);

    [HttpPost("{id:int}/versions/{versionId:int}/promote")]
    [Authorize(Policy = PermissionCatalog.WorkflowsPublish)]
    public async Task<IActionResult> PromoteVersion(int id, int versionId, PromoteVersionRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new PromoteWorkflowVersionCommand(id, versionId, request.TargetStage), cancellationToken);
        return FromResult(result);
    }

    public record ExecuteWorkflowRequest(string? InputPayloadJson);

    [HttpPost("{id:int}/execute")]
    [Authorize(Policy = PermissionCatalog.WorkflowsExecute)]
    public async Task<IActionResult> Execute(int id, ExecuteWorkflowRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ExecuteWorkflowCommand(id, request.InputPayloadJson), cancellationToken);
        return FromResult(result, executionId => Ok(new { executionId }));
    }

    [HttpGet("{id:int}/executions")]
    [Authorize(Policy = PermissionCatalog.ExecutionsRead)]
    public async Task<IActionResult> GetExecutionHistory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetExecutionHistoryQuery(id, page, pageSize), cancellationToken);
        return FromResult(result);
    }
}
