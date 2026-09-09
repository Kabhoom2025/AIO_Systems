using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Users.Commands.ConfirmUserEmail;

public class ConfirmUserEmailCommandHandler : IRequestHandler<ConfirmUserEmailCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public ConfirmUserEmailCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(ConfirmUserEmailCommand request, CancellationToken cancellationToken)
    {
        // Users is ITenantScoped, so this lookup is already implicitly restricted to the
        // caller's own organization by the global query filter.
        var target = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (target is null)
        {
            return Result.Failure(Error.NotFound("User not found."));
        }

        if (target.IsEmailVerified)
        {
            return Result.Success();
        }

        target.IsEmailVerified = true;
        target.EmailVerificationToken = null;
        target.EmailVerificationTokenExpiresAt = null;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
