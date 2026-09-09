using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Executions.Commands.CompleteUserTask;

public record CompleteUserTaskCommand(Guid ExecutionId, bool Approved, string? Comment) : IRequest<Result>;
