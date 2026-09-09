using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workflows.Queries.GetWorkflowVersionById;

public class GetWorkflowVersionByIdQueryHandler : IRequestHandler<GetWorkflowVersionByIdQuery, Result<WorkflowVersionDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetWorkflowVersionByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<WorkflowVersionDetailDto>> Handle(GetWorkflowVersionByIdQuery request, CancellationToken cancellationToken)
    {
        var version = await _db.WorkflowVersions
            .Where(v => v.Id == request.VersionId && v.WorkflowDefinitionId == request.WorkflowDefinitionId)
            .Select(v => new { v.Id, v.VersionNumber, v.Status, v.GraphJson, v.PublishedAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (version is null)
        {
            return Result<WorkflowVersionDetailDto>.Failure(Error.NotFound($"Version {request.VersionId} was not found."));
        }

        return Result<WorkflowVersionDetailDto>.Success(new WorkflowVersionDetailDto(
            version.Id, version.VersionNumber, version.Status.ToString(), version.GraphJson, version.PublishedAt));
    }
}
