using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Application.Specifications;
using FlowSphere.Application.Workflows.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workflows.Queries.GetWorkflowsList;

public class GetWorkflowsListQueryHandler : IRequestHandler<GetWorkflowsListQuery, Result<PagedResult<WorkflowSummaryDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetWorkflowsListQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<WorkflowSummaryDto>>> Handle(GetWorkflowsListQuery request, CancellationToken cancellationToken)
    {
        var spec = new WorkflowsByOrganizationSpecification(_currentUser.OrganizationId, request.Page, request.PageSize);

        var totalCount = await _db.WorkflowDefinitions.CountAsync(w => w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        var items = await SpecificationEvaluator.GetQuery(_db.WorkflowDefinitions.AsQueryable(), spec)
            .Select(w => new WorkflowSummaryDto(w.Id, w.Name, w.Description, w.IsEnabled, w.CreatedDate))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<WorkflowSummaryDto>>.Success(new PagedResult<WorkflowSummaryDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
