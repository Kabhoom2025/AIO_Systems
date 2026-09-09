using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Executions.Commands.CompleteUserTask;

public class CompleteUserTaskCommandHandler : IRequestHandler<CompleteUserTaskCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IExecutionQueue _queue;
    private readonly ICurrentUserContext _currentUser;

    public CompleteUserTaskCommandHandler(IApplicationDbContext db, IExecutionQueue queue, ICurrentUserContext currentUser)
    {
        _db = db;
        _queue = queue;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(CompleteUserTaskCommand request, CancellationToken cancellationToken)
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

        var stepLog = await _db.ExecutionStepLogs
            .Where(s => s.WorkflowExecutionId == execution.Id
                && s.NodeKey == execution.PendingNodeKey
                && s.Status == StepStatus.WaitingApproval)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (stepLog is null)
        {
            return Result.Failure(Error.NotFound("No pending User Task step was found for this execution."));
        }

        // An empty/missing Roles list on the Step means unrestricted - preserves today's
        // behavior (anyone with workflows.approve may resolve it) for every workflow authored
        // before this field existed.
        var version = await _db.WorkflowVersions
            .Where(v => v.Id == execution.WorkflowVersionId)
            .Select(v => v.GraphJson)
            .FirstOrDefaultAsync(cancellationToken);

        var allowedRoles = version is null ? Array.Empty<string>() : WorkflowGraphNodeConfig.GetRoles(version, execution.PendingNodeKey);

        if (allowedRoles.Length > 0)
        {
            var currentUserRoleName = await _db.Users
                .Where(u => u.Id == _currentUser.UserId)
                .Select(u => u.Role != null ? u.Role.Name : null)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentUserRoleName is null || !allowedRoles.Contains(currentUserRoleName))
            {
                return Result.Failure(Error.Unauthorized("This step can only be resolved by a user with one of its assigned roles."));
            }
        }

        stepLog.Status = StepStatus.Succeeded;
        stepLog.OutputJson = JsonSerializer.Serialize(new { approved = request.Approved, comment = request.Comment });
        stepLog.CompletedAt = DateTime.UtcNow;

        // Status stays PendingApproval in the DB record's spirit until the engine actually
        // resumes and calls ResumeFromPending() - but it must not stay PendingApproval (a client
        // could re-submit while the resume job is still queued), so it moves to Queued here as
        // "queued to resume", the same status a brand-new execution starts in.
        execution.Status = ExecutionStatus.Queued;
        execution.PendingResumeHandle = request.Approved ? "approved" : "rejected";

        await _db.SaveChangesAsync(cancellationToken);
        await _queue.EnqueueAsync(execution.Id, cancellationToken);

        return Result.Success();
    }
}
