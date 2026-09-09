using LinkShield.Application.DTOs.Auth;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Entities;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private const int MaxFailedLogins = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan EmailVerificationTokenLifetime = TimeSpan.FromHours(24);
    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);

    private readonly LinkShieldDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IEmailSender _emailSender;

    public AuthService(
        LinkShieldDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IEmailSender emailSender)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _emailSender = emailSender;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct))
            throw new InvalidOperationException("A user with this email already exists.");

        var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == SystemRole.User, ct)
            ?? throw new InvalidOperationException("Default role 'User' is not seeded.");

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailVerificationToken = _tokenGenerator.GenerateRefreshToken(),
            EmailVerificationTokenExpiresAtUtc = DateTime.UtcNow.Add(EmailVerificationTokenLifetime)
        };
        _db.Users.Add(user);
        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = defaultRole.Id });
        _db.AuditLogs.Add(new AuditLog { UserId = user.Id, Action = AuditAction.Register, EntityType = nameof(User), EntityId = user.Id.ToString() });
        await _db.SaveChangesAsync(ct);

        await _emailSender.SendAsync(
            user.Email,
            "Verify your LinkShield AI account",
            $"Your email verification token is: {user.EmailVerificationToken}");

        return await IssueTokensAsync(user, ipAddress: null, ct);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.LockedUntilUtc.HasValue && user.LockedUntilUtc > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account is temporarily locked due to repeated failed logins.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedLogins)
            {
                user.LockedUntilUtc = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginAttempts = 0;
            }
            await _db.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.FailedLoginAttempts = 0;
        user.LockedUntilUtc = null;
        user.LastLoginAtUtc = DateTime.UtcNow;
        _db.AuditLogs.Add(new AuditLog { UserId = user.Id, Action = AuditAction.Login, EntityType = nameof(User), EntityId = user.Id.ToString(), IpAddress = ipAddress });

        return await IssueTokensAsync(user, ipAddress, ct);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken ct = default)
    {
        var stored = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (stored is null || !stored.IsActive || !stored.User.IsActive)
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");

        var newToken = _tokenGenerator.GenerateRefreshToken();
        stored.RevokedAtUtc = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress ?? string.Empty;
        stored.ReplacedByToken = newToken;

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = stored.UserId,
            Token = newToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_tokenGenerator.RefreshTokenLifetimeDays),
            CreatedByIp = ipAddress ?? string.Empty
        });
        await _db.SaveChangesAsync(ct);

        return await BuildResponseAsync(stored.User, newToken, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken, ct);
        if (stored is null || stored.RevokedAtUtc.HasValue) return;

        stored.RevokedAtUtc = DateTime.UtcNow;
        _db.AuditLogs.Add(new AuditLog { UserId = stored.UserId, Action = AuditAction.Logout, EntityType = nameof(User), EntityId = stored.UserId.ToString() });
        await _db.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive, ct);
        if (user is null) return; // never reveal whether the email exists

        user.PasswordResetToken = _tokenGenerator.GenerateRefreshToken();
        user.PasswordResetTokenExpiresAtUtc = DateTime.UtcNow.Add(PasswordResetTokenLifetime);
        await _db.SaveChangesAsync(ct);

        await _emailSender.SendAsync(
            user.Email,
            "Reset your LinkShield AI password",
            $"Your password reset token is: {user.PasswordResetToken}");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token, ct);
        if (user is null || user.PasswordResetTokenExpiresAtUtc is null || user.PasswordResetTokenExpiresAtUtc < DateTime.UtcNow)
            throw new InvalidOperationException("Password reset token is invalid or expired.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAtUtc = null;
        _db.AuditLogs.Add(new AuditLog { UserId = user.Id, Action = AuditAction.PasswordReset, EntityType = nameof(User), EntityId = user.Id.ToString() });
        await _db.SaveChangesAsync(ct);
    }

    public async Task VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token, ct);
        if (user is null || user.EmailVerificationTokenExpiresAtUtc is null || user.EmailVerificationTokenExpiresAtUtc < DateTime.UtcNow)
            throw new InvalidOperationException("Email verification token is invalid or expired.");

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAtUtc = null;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, string? ipAddress, CancellationToken ct)
    {
        var refreshToken = _tokenGenerator.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_tokenGenerator.RefreshTokenLifetimeDays),
            CreatedByIp = ipAddress ?? string.Empty
        });
        await _db.SaveChangesAsync(ct);

        return await BuildResponseAsync(user, refreshToken, ct);
    }

    private async Task<AuthResponseDto> BuildResponseAsync(User user, string refreshToken, CancellationToken ct)
    {
        var roles = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name.ToString())
            .ToListAsync(ct);

        var (accessToken, expiresAtUtc) = _tokenGenerator.GenerateAccessToken(user, roles);

        return new AuthResponseDto(
            accessToken,
            refreshToken,
            expiresAtUtc,
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            roles);
    }
}
