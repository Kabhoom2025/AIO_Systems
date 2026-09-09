using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Users.Commands.SetUserActive;

public class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public SetUserActiveCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        // Users is ITenantScoped, so this lookup is already implicitly restricted to the
        // caller's own organization by the global query filter.
        var target = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (target is null)
        {
            return Result.Failure(Error.NotFound("User not found."));
        }

        if (target.Id == _currentUser.UserId)
        {
            return Result.Failure(Error.Conflict("You cannot deactivate your own account."));
        }

        if (!request.IsActive && target.Role?.Name == "Admin")
        {
            var activeAdminCount = await _db.Users
                .CountAsync(u => u.RoleId == target.RoleId && u.IsActive, cancellationToken);

            if (activeAdminCount <= 1)
            {
                return Result.Failure(Error.Conflict("Cannot deactivate the last admin in the organization."));
            }
        }

        target.IsActive = request.IsActive;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
