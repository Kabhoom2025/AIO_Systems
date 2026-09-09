using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workflows.Queries.GetWorkflowVersionsList;

public class GetWorkflowVersionsListQueryHandler : IRequestHandler<GetWorkflowVersionsListQuery, Result<List<WorkflowVersionSummaryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetWorkflowVersionsListQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<WorkflowVersionSummaryDto>>> Handle(GetWorkflowVersionsListQuery request, CancellationToken cancellationToken)
    {
        // Projected without .ToString() on the enum column - some providers can't translate
        // that inside SQL, so the enum -> string conversion happens client-side after materializing.
        var rows = await _db.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == request.WorkflowDefinitionId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new { v.Id, v.VersionNumber, v.Status, v.PublishedAt, v.CreatedDate })
            .ToListAsync(cancellationToken);

        var versions = rows
            .Select(v => new WorkflowVersionSummaryDto(v.Id, v.VersionNumber, v.Status.ToString(), v.PublishedAt, v.CreatedDate))
            .ToList();

        return Result<List<WorkflowVersionSummaryDto>>.Success(versions);
    }
}
