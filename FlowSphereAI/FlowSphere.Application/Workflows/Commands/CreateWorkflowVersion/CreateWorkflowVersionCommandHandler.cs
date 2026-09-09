using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workflows.Commands.CreateWorkflowVersion;

public class CreateWorkflowVersionCommandHandler : IRequestHandler<CreateWorkflowVersionCommand, Result<CreatedVersionDto>>
{
    private readonly IApplicationDbContext _db;

    public CreateWorkflowVersionCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CreatedVersionDto>> Handle(CreateWorkflowVersionCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _db.WorkflowDefinitions
            .Include(w => w.Versions)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowDefinitionId, cancellationToken);

        if (workflow is null)
        {
            return Result<CreatedVersionDto>.Failure(Error.NotFound($"Workflow {request.WorkflowDefinitionId} was not found."));
        }

        var version = workflow.CreateNextVersion(request.GraphJson);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CreatedVersionDto>.Success(new CreatedVersionDto(version.Id, version.VersionNumber));
    }
}
