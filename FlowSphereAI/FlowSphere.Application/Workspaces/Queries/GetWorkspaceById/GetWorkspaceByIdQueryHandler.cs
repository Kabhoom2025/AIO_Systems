using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.Queries.GetWorkspaceById;

public class GetWorkspaceByIdQueryHandler : IRequestHandler<GetWorkspaceByIdQuery, Result<WorkspaceDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetWorkspaceByIdQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkspaceDetailDto>> Handle(GetWorkspaceByIdQuery request, CancellationToken cancellationToken)
    {
        var workspace = await _db.Workspaces
            .Where(w => w.Id == request.Id && w.OrganizationId == _currentUser.OrganizationId)
            .Select(w => new WorkspaceDetailDto(w.Id, w.Name, w.Description, w.CreatedDate))
            .FirstOrDefaultAsync(cancellationToken);

        return workspace is null
            ? Result<WorkspaceDetailDto>.Failure(Error.NotFound($"Workspace {request.Id} was not found."))
            : Result<WorkspaceDetailDto>.Success(workspace);
    }
}
