using System.Security.Cryptography;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowSphere.Application.Users.Commands.InviteUser;

public class InviteUserCommandHandler : IRequestHandler<InviteUserCommand, Result<InviteUserResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ClientSettings _clientSettings;
    private readonly ILogger<InviteUserCommandHandler> _logger;

    public InviteUserCommandHandler(
        IApplicationDbContext db,
        ICurrentUserContext currentUser,
        IPasswordHasher passwordHasher,
        IOptions<ClientSettings> clientSettings,
        ILogger<InviteUserCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _clientSettings = clientSettings.Value;
        _logger = logger;
    }

    public async Task<Result<InviteUserResult>> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        var emailTaken = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailTaken)
        {
            return Result<InviteUserResult>.Failure(Error.Conflict("An account with this email already exists."));
        }

        if (request.ManagerId is not null)
        {
            var managerExists = await _db.Users.AnyAsync(u => u.Id == request.ManagerId, cancellationToken);
            if (!managerExists)
            {
                return Result<InviteUserResult>.Failure(Error.NotFound("The selected manager was not found."));
            }
        }

        if (request.RoleName is null)
        {
            var verificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            var pendingUser = new User
            {
                OrganizationId = _currentUser.OrganizationId,
                Name = request.Name,
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                ManagerId = request.ManagerId,
                RoleId = null,
                IsActive = true,
                IsEmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
            };
            _db.Users.Add(pendingUser);
            await _db.SaveChangesAsync(cancellationToken);

            var verificationLink = $"{_clientSettings.BaseUrl}/verify-email?token={verificationToken}";
            _logger.LogInformation("Email verification link for {Email}: {Link}", pendingUser.Email, verificationLink);

            return Result<InviteUserResult>.Success(new InviteUserResult(pendingUser.Id, pendingUser.Name, pendingUser.Email, null, false));
        }

        var role = await _db.Roles
            .FirstOrDefaultAsync(r => r.OrganizationId == _currentUser.OrganizationId && r.Name == request.RoleName, cancellationToken);

        if (role is null)
        {
            role = new Role
            {
                OrganizationId = _currentUser.OrganizationId,
                Name = request.RoleName,
                Permissions = string.Join(',', PermissionCatalog.PermissionsForRoleName(request.RoleName)),
            };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var user = new User
        {
            OrganizationId = _currentUser.OrganizationId,
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            ManagerId = request.ManagerId,
            RoleId = role.Id,
            IsActive = true,
            // Invited by an admin who already vouches for them, unlike self-service registration.
            IsEmailVerified = true,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<InviteUserResult>.Success(new InviteUserResult(user.Id, user.Name, user.Email, role.Name, true));
    }
}
