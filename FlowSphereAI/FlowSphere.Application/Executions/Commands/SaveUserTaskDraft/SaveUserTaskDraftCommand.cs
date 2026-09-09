using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Executions.Commands.SaveUserTaskDraft;

/// <summary>Persists interim form input for a pending Step without resolving it - the execution
/// stays PendingApproval/queued exactly as before. Only meaningful when the Step has
/// "Enable Save As Draft" on; the API/UI decide whether to offer this, the handler itself just
/// requires the execution to genuinely be pending.</summary>
public record SaveUserTaskDraftCommand(Guid ExecutionId, string DraftDataJson) : IRequest<Result>;
