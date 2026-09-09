using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workflows.Commands.ExecuteWorkflow;

public class ExecuteWorkflowCommandHandler : IRequestHandler<ExecuteWorkflowCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IExecutionQueue _queue;

    public ExecuteWorkflowCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, IExecutionQueue queue)
    {
        _db = db;
        _currentUser = currentUser;
        _queue = queue;
    }

    public async Task<Result<Guid>> Handle(ExecuteWorkflowCommand request, CancellationToken cancellationToken)
    {
        var version = await _db.WorkflowVersions
            .FirstOrDefaultAsync(v => v.WorkflowDefinitionId == request.WorkflowDefinitionId && v.Status == VersionStatus.Published, cancellationToken);

        if (version is null)
        {
            return Result<Guid>.Failure(Error.NotFound("This workflow has no published version to execute."));
        }

        var organization = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == _currentUser.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<Guid>.Failure(Error.NotFound("Organization not found."));
        }

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        // WorkflowExecutions is ITenantScoped, so this count is already implicitly filtered to
        // the current organization by the global query filter - no explicit OrganizationId check needed.
        var executionsThisMonth = await _db.WorkflowExecutions
            .CountAsync(e => e.CreatedDate >= monthStart, cancellationToken);

        if (executionsThisMonth >= organization.MonthlyExecutionQuota)
        {
            return Result<Guid>.Failure(Error.QuotaExceeded(
                $"Monthly execution quota of {organization.MonthlyExecutionQuota} reached for the {organization.PlanTier} plan."));
        }

        var execution = WorkflowExecution.CreateQueued(_currentUser.OrganizationId, version, request.InputPayloadJson);
        _db.WorkflowExecutions.Add(execution);
        await _db.SaveChangesAsync(cancellationToken);

        await _queue.EnqueueAsync(execution.Id, cancellationToken);

        return Result<Guid>.Success(execution.Id);
    }
}
