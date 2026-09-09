using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workflows.Commands.PublishWorkflowVersion;

public class PublishWorkflowVersionCommandHandler : IRequestHandler<PublishWorkflowVersionCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public PublishWorkflowVersionCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(PublishWorkflowVersionCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _db.WorkflowDefinitions
            .Include(w => w.Versions)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowDefinitionId, cancellationToken);

        if (workflow is null)
        {
            return Result.Failure(Error.NotFound($"Workflow {request.WorkflowDefinitionId} was not found."));
        }

        try
        {
            workflow.Publish(request.VersionId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(Error.NotFound(ex.Message));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
