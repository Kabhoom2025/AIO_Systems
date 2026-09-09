using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Executions.Queries.GetExecutionDetail;

public class GetExecutionDetailQueryHandler : IRequestHandler<GetExecutionDetailQuery, Result<ExecutionDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetExecutionDetailQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ExecutionDetailDto>> Handle(GetExecutionDetailQuery request, CancellationToken cancellationToken)
    {
        var execution = await _db.WorkflowExecutions
            .Include(e => e.StepLogs)
            .Include(e => e.WorkflowVersion)
            .FirstOrDefaultAsync(e => e.Id == request.ExecutionId, cancellationToken);

        if (execution is null)
        {
            return Result<ExecutionDetailDto>.Failure(Error.NotFound($"Execution {request.ExecutionId} was not found."));
        }

        var steps = execution.StepLogs
            .OrderBy(s => s.StartedAt)
            .Select(s => new ExecutionStepLogDto(
                s.Id, s.NodeKey, s.NodeType.ToString(), s.Status.ToString(), s.AttemptNumber,
                s.StartedAt, s.CompletedAt, s.OutputJson, s.ErrorMessage))
            .ToList();

        var graphJson = execution.WorkflowVersion.GraphJson;
        var pendingAssigneeLabel = WorkflowGraphNodeConfig.GetAssigneeLabel(graphJson, execution.PendingNodeKey);
        var pendingNodeConfigJson = WorkflowGraphNodeConfig.GetNodeConfigJson(graphJson, execution.PendingNodeKey);
        var pendingActionsJson = WorkflowGraphNodeConfig.GetOutgoingActionsJson(graphJson, execution.PendingNodeKey);

        return Result<ExecutionDetailDto>.Success(new ExecutionDetailDto(
            execution.Id, execution.WorkflowDefinitionId, execution.WorkflowVersionId, execution.Status.ToString(),
            execution.TriggerSource, execution.StartedAt, execution.CompletedAt, execution.ErrorSummary,
            execution.PendingNodeKey, pendingAssigneeLabel, pendingNodeConfigJson, pendingActionsJson,
            execution.PendingDraftDataJson, steps));
    }
}
