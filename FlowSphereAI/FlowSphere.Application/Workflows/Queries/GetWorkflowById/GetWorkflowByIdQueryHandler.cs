using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workflows.Queries.GetWorkflowById;

public class GetWorkflowByIdQueryHandler : IRequestHandler<GetWorkflowByIdQuery, Result<WorkflowDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetWorkflowByIdQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkflowDetailDto>> Handle(GetWorkflowByIdQuery request, CancellationToken cancellationToken)
    {
        var workflow = await _db.WorkflowDefinitions
            .Where(w => w.Id == request.Id && w.OrganizationId == _currentUser.OrganizationId)
            .Select(w => new WorkflowDetailDto(w.Id, w.Name, w.Description, w.IsEnabled, w.CreatedDate))
            .FirstOrDefaultAsync(cancellationToken);

        return workflow is null
            ? Result<WorkflowDetailDto>.Failure(Error.NotFound($"Workflow {request.Id} was not found."))
            : Result<WorkflowDetailDto>.Success(workflow);
    }
}
