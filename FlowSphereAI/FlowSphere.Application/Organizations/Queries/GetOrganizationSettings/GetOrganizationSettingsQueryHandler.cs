using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Organizations.Queries.GetOrganizationSettings;

public class GetOrganizationSettingsQueryHandler : IRequestHandler<GetOrganizationSettingsQuery, Result<OrganizationSettingsDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetOrganizationSettingsQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<OrganizationSettingsDto>> Handle(GetOrganizationSettingsQuery request, CancellationToken cancellationToken)
    {
        var organization = await _db.Organizations
            .Where(o => o.Id == _currentUser.OrganizationId)
            .Select(o => new OrganizationSettingsDto(o.Name, o.SettingsJson))
            .FirstOrDefaultAsync(cancellationToken);

        return organization is null
            ? Result<OrganizationSettingsDto>.Failure(Error.NotFound("Organization not found."))
            : Result<OrganizationSettingsDto>.Success(organization);
    }
}
