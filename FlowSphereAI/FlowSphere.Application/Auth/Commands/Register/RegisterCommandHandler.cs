using System.Security.Cryptography;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowSphere.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ClientSettings _clientSettings;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<ClientSettings> clientSettings,
        ILogger<RegisterCommandHandler> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _clientSettings = clientSettings.Value;
        _logger = logger;
    }

    public async Task<Result<RegisterResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Pre-auth, same as LoginCommandHandler: no OrganizationId exists yet for this caller,
        // so email-uniqueness must be checked across every tenant, not just one.
        var emailTaken = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailTaken)
        {
            return Result<RegisterResult>.Failure(Error.Conflict("An account with this email already exists."));
        }

        var organization = new Organization { Name = request.OrganizationName, IsActive = true };
        _db.Organizations.Add(organization);
        await _db.SaveChangesAsync(cancellationToken);

        var adminRole = new Role
        {
            OrganizationId = organization.Id,
            Name = "Admin",
            Permissions = string.Join(',', PermissionCatalog.All),
        };
        _db.Roles.Add(adminRole);
        await _db.SaveChangesAsync(cancellationToken);

        var verificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        var user = new User
        {
            OrganizationId = organization.Id,
            Name = request.AdminName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            RoleId = adminRole.Id,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        // No transactional email provider is wired up yet - this module has no platform-level
        // SMTP config (the Smtp connector is per-organization and used only by workflow nodes).
        // Logging the link is a stand-in until real email delivery is added; RegisterResult also
        // returns it directly so the flow is genuinely testable end-to-end in the meantime.
        var verificationLink = $"{_clientSettings.BaseUrl}/verify-email?token={verificationToken}";
        _logger.LogInformation("Email verification link for {Email}: {Link}", user.Email, verificationLink);

        var token = _jwtTokenGenerator.GenerateToken(user, adminRole);

        return Result<RegisterResult>.Success(new RegisterResult(
            token,
            user.Name,
            user.Email,
            organization.Id,
            adminRole.PermissionList.ToArray(),
            user.IsEmailVerified,
            verificationLink,
            adminRole.Name));
    }
}
