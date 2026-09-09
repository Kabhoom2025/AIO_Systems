using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, IPasswordHasher passwordHasher)
    {
        _db = db;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            // Not Error.Unauthorized: that maps to HTTP 401, which the frontend's apiFetch
            // interprets as "session expired" and force-logs the user out - a wrong password
            // here is a validation failure of the request, not an invalid/expired JWT.
            return Result.Failure(Error.Conflict("Current password is incorrect.", "InvalidCurrentPassword"));
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
