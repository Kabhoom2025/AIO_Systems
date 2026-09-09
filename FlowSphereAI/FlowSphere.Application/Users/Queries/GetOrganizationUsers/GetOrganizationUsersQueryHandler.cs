using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Users.Queries.GetOrganizationUsers;

public class GetOrganizationUsersQueryHandler : IRequestHandler<GetOrganizationUsersQuery, Result<IReadOnlyList<OrganizationUserDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetOrganizationUsersQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<OrganizationUserDto>>> Handle(GetOrganizationUsersQuery request, CancellationToken cancellationToken)
    {
        // Users is ITenantScoped, so this is already implicitly filtered to the caller's
        // organization by the global query filter.
        var users = await _db.Users
            .Include(u => u.Role)
            .Include(u => u.Manager)
            .OrderBy(u => u.Name)
            .Select(u => new OrganizationUserDto(
                u.Id,
                u.Name,
                u.Email,
                u.Role != null ? u.Role.Name : null,
                u.IsActive,
                u.IsEmailVerified,
                u.Manager != null ? u.Manager.Name : null,
                (!u.IsEmailVerified || u.RoleId == null) ? "Pending" : (u.IsActive ? "Active" : "Inactive"),
                u.LastLoginAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<OrganizationUserDto>>.Success(users);
    }
}
