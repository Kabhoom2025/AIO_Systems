using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Executions.Queries.GetExecutionDetail;

public record GetExecutionDetailQuery(Guid ExecutionId) : IRequest<Result<ExecutionDetailDto>>;

public record ExecutionStepLogDto(int Id, string NodeKey, string NodeType, string Status, int AttemptNumber,
    DateTime? StartedAt, DateTime? CompletedAt, string? OutputJson, string? ErrorMessage);

public record ExecutionDetailDto(Guid Id, int WorkflowDefinitionId, int WorkflowVersionId, string Status,
    string TriggerSource, DateTime? StartedAt, DateTime? CompletedAt, string? ErrorSummary,
    string? PendingNodeKey, string? PendingAssigneeLabel, string? PendingNodeConfigJson,
    string PendingActionsJson, string? PendingDraftDataJson, List<ExecutionStepLogDto> Steps);
