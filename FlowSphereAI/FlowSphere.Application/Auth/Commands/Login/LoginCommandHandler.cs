using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Pre-auth: we don't yet know the caller's OrganizationId, so the tenant query filter
        // must be explicitly bypassed here - this is the one legitimate IgnoreQueryFilters()
        // call site in the whole module (see Section 5 of the plan).
        var user = await _db.Users
            .IgnoreQueryFilters()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResult>.Failure(Error.Unauthorized("Invalid email or password."));
        }

        if (user.Role is null)
        {
            return Result<LoginResult>.Failure(Error.Unauthorized("Your account is pending activation. Contact an administrator to confirm your email and assign a role."));
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user, user.Role);

        return Result<LoginResult>.Success(new LoginResult(
            token,
            user.Name,
            user.Email,
            user.OrganizationId,
            user.Role.PermissionList.ToArray(),
            user.IsEmailVerified,
            user.Role.Name));
    }
}
