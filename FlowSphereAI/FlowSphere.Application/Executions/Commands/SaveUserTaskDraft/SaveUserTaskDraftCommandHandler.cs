using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Executions.Commands.SaveUserTaskDraft;

public class SaveUserTaskDraftCommandHandler : IRequestHandler<SaveUserTaskDraftCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SaveUserTaskDraftCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(SaveUserTaskDraftCommand request, CancellationToken cancellationToken)
    {
        var execution = await _db.WorkflowExecutions
            .FirstOrDefaultAsync(e => e.Id == request.ExecutionId, cancellationToken);

        if (execution is null)
        {
            return Result.Failure(Error.NotFound($"Execution {request.ExecutionId} was not found."));
        }

        if (execution.Status != ExecutionStatus.PendingApproval || execution.PendingNodeKey is null)
        {
            return Result.Failure(Error.Conflict("This execution is not waiting on a User Task decision."));
        }

        execution.SaveDraft(request.DraftDataJson);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
