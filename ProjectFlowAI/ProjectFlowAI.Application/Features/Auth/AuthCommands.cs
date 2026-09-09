using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Auth;

// ---------------------------------------------------------------------------------------------
// Register
// ---------------------------------------------------------------------------------------------
public record RegisterCommand(string Email, string Password, string FirstName, string LastName) : IRequest<AuthResultDto>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailSender _email;

    public RegisterCommandHandler(IProjectFlowDbContext db, IPasswordHasher hasher, ITokenService tokens,
        IDateTimeProvider clock, IEmailSender email)
    {
        _db = db; _hasher = hasher; _tokens = tokens; _clock = clock; _email = email;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
            throw new ConflictException($"A user with email '{normalizedEmail}' already exists.");

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = _hasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Status = UserStatus.Active,
            IsEmailVerified = false,
            CreatedAt = _clock.UtcNow
        };
        _db.Users.Add(user);

        var verificationRaw = _tokens.GenerateRefreshToken();
        _db.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = _tokens.HashToken(verificationRaw),
            ExpiresAt = _clock.UtcNow.AddDays(2)
        });

        await _db.SaveChangesAsync(cancellationToken);

        await _email.SendAsync(user.Email, "Verify your ProjectFlow AI account",
            $"Welcome to ProjectFlow AI! Your verification code is: {verificationRaw}", cancellationToken);

        // A freshly registered user genuinely has no roles/org yet — empty lists here match
        // reality, not a bug (contrast with the other AuthResultDto-returning handlers below,
        // which look up the user's real assigned roles/permissions).
        var noRoles = new List<string>();
        var noPermissions = new List<string>();
        var access = _tokens.GenerateAccessToken(user, noRoles, noPermissions);
        var refreshRaw = _tokens.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokens.HashToken(refreshRaw),
            ExpiresAt = _clock.UtcNow.AddDays(7),
            CreatedAt = _clock.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(access, refreshRaw, _clock.UtcNow.AddMinutes(30), ToDto(user, noRoles, noPermissions));
    }

    // Roles/Permissions are supplied by the caller (already computed for the access token)
    // rather than recomputed here — every AuthResultDto-producing handler needs both anyway,
    // and UserDto has no way to look them up itself (see MappingProfile's comment on why
    // there's no CreateMap<User, UserDto>).
    internal static UserDto ToDto(User u, IReadOnlyList<string> roles, IReadOnlyList<string> permissions) => new(
        u.Id, u.Email, u.FirstName, u.LastName, u.AvatarUrl,
        u.IsEmailVerified, u.TwoFactorEnabled, u.Status, u.LastLoginAt, u.OrganizationId, u.CreatedAt,
        roles, permissions);
}

// ---------------------------------------------------------------------------------------------
// Login
// ---------------------------------------------------------------------------------------------
public record LoginCommand(string Email, string Password, string? IpAddress, string? TwoFactorCode = null) : IRequest<AuthResultDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IDateTimeProvider _clock;
    private readonly ITotpService _totp;

    public LoginCommandHandler(IProjectFlowDbContext db, IPasswordHasher hasher, ITokenService tokens,
        IDateTimeProvider clock, ITotpService totp)
    {
        _db = db; _hasher = hasher; _tokens = tokens; _clock = clock; _totp = totp;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user == null || !_hasher.VerifyPassword(user.PasswordHash, request.Password))
            throw new UnauthorizedDomainException("Invalid email or password.");

        if (user.Status != UserStatus.Active)
            throw new UnauthorizedDomainException($"Account is {user.Status}.");

        if (user.TwoFactorEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.TwoFactorCode) || !_totp.ValidateCode(user.TwoFactorSecret ?? string.Empty, request.TwoFactorCode))
                throw new UnauthorizedDomainException("A valid two-factor code is required.");
        }

        var (roles, permissions) = await UserAuthorizationHelper.GetRolesAndPermissionsAsync(_db, user.Id, cancellationToken);
        var access = _tokens.GenerateAccessToken(user, roles, permissions);
        var refreshRaw = _tokens.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokens.HashToken(refreshRaw),
            ExpiresAt = _clock.UtcNow.AddDays(7),
            CreatedAt = _clock.UtcNow,
            CreatedByIp = request.IpAddress
        });

        user.LastLoginAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(access, refreshRaw, _clock.UtcNow.AddMinutes(30), RegisterCommandHandler.ToDto(user, roles, permissions));
    }
}

// ---------------------------------------------------------------------------------------------
// RefreshToken
// ---------------------------------------------------------------------------------------------
public record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<AuthResultDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IDateTimeProvider _clock;

    public RefreshTokenCommandHandler(IProjectFlowDbContext db, ITokenService tokens, IDateTimeProvider clock)
    {
        _db = db; _tokens = tokens; _clock = clock;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokens.HashToken(request.RefreshToken);
        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken);
        if (existing == null || existing.RevokedAt != null || existing.ExpiresAt < _clock.UtcNow)
            throw new UnauthorizedDomainException("Invalid or expired refresh token.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == existing.UserId, cancellationToken)
            ?? throw new NotFoundException("User", existing.UserId);

        var newRaw = _tokens.GenerateRefreshToken();
        var newToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokens.HashToken(newRaw),
            ExpiresAt = _clock.UtcNow.AddDays(7),
            CreatedAt = _clock.UtcNow,
            CreatedByIp = request.IpAddress
        };
        _db.RefreshTokens.Add(newToken);

        existing.RevokedAt = _clock.UtcNow;
        existing.ReplacedByTokenId = newToken.Id;

        var (roles, permissions) = await UserAuthorizationHelper.GetRolesAndPermissionsAsync(_db, user.Id, cancellationToken);
        var access = _tokens.GenerateAccessToken(user, roles, permissions);

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(access, newRaw, _clock.UtcNow.AddMinutes(30), RegisterCommandHandler.ToDto(user, roles, permissions));
    }
}

// ---------------------------------------------------------------------------------------------
// Logout
// ---------------------------------------------------------------------------------------------
public record LogoutCommand(string RefreshToken) : IRequest<Unit>;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IDateTimeProvider _clock;

    public LogoutCommandHandler(IProjectFlowDbContext db, ITokenService tokens, IDateTimeProvider clock)
    {
        _db = db; _tokens = tokens; _clock = clock;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokens.HashToken(request.RefreshToken);
        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken);
        if (existing != null && existing.RevokedAt == null)
        {
            existing.RevokedAt = _clock.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------------------------
// VerifyEmail
// ---------------------------------------------------------------------------------------------
public record VerifyEmailCommand(Guid UserId, string Token) : IRequest<Unit>;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IDateTimeProvider _clock;

    public VerifyEmailCommandHandler(IProjectFlowDbContext db, ITokenService tokens, IDateTimeProvider clock)
    {
        _db = db; _tokens = tokens; _clock = clock;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokens.HashToken(request.Token);
        var token = await _db.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.UserId == request.UserId && t.TokenHash == hash, cancellationToken);
        if (token == null || token.UsedAt != null || token.ExpiresAt < _clock.UtcNow)
            throw new UnauthorizedDomainException("Invalid or expired verification token.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.IsEmailVerified = true;
        token.UsedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------------------------
// ForgotPassword
// ---------------------------------------------------------------------------------------------
public record ForgotPasswordCommand(string Email) : IRequest<Unit>;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailSender _email;

    public ForgotPasswordCommandHandler(IProjectFlowDbContext db, ITokenService tokens, IDateTimeProvider clock, IEmailSender email)
    {
        _db = db; _tokens = tokens; _clock = clock; _email = email;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        // Deliberately do not reveal whether the account exists.
        if (user == null) return Unit.Value;

        var raw = _tokens.GenerateRefreshToken();
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = _tokens.HashToken(raw),
            ExpiresAt = _clock.UtcNow.AddHours(1)
        });
        await _db.SaveChangesAsync(cancellationToken);

        await _email.SendAsync(user.Email, "Reset your ProjectFlow AI password",
            $"Your password reset code is: {raw} (expires in 1 hour)", cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------------------------
// ResetPassword
// ---------------------------------------------------------------------------------------------
public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Unit>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").Matches("[a-z]").Matches("[0-9]");
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IPasswordHasher _hasher;
    private readonly IDateTimeProvider _clock;

    public ResetPasswordCommandHandler(IProjectFlowDbContext db, ITokenService tokens, IPasswordHasher hasher, IDateTimeProvider clock)
    {
        _db = db; _tokens = tokens; _hasher = hasher; _clock = clock;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken)
            ?? throw new UnauthorizedDomainException("Invalid reset request.");

        var hash = _tokens.HashToken(request.Token);
        var token = await _db.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.TokenHash == hash, cancellationToken);
        if (token == null || token.UsedAt != null || token.ExpiresAt < _clock.UtcNow)
            throw new UnauthorizedDomainException("Invalid or expired reset token.");

        user.PasswordHash = _hasher.HashPassword(request.NewPassword);
        token.UsedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------------------------
// Two-factor
// ---------------------------------------------------------------------------------------------
public record EnableTwoFactorCommand(Guid UserId) : IRequest<string>;

public class EnableTwoFactorCommandValidator : AbstractValidator<EnableTwoFactorCommand>
{
    public EnableTwoFactorCommandValidator() => RuleFor(x => x.UserId).NotEmpty();
}

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, string>
{
    private readonly IProjectFlowDbContext _db;
    private readonly ITotpService _totp;

    public EnableTwoFactorCommandHandler(IProjectFlowDbContext db, ITotpService totp)
    {
        _db = db; _totp = totp;
    }

    public async Task<string> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        var secret = _totp.GenerateSecret();
        user.TwoFactorSecret = secret;
        // TwoFactorEnabled flips to true only once VerifyTwoFactorCommand confirms the user has the secret.
        await _db.SaveChangesAsync(cancellationToken);

        return _totp.GenerateQrCodeUri(secret, user.Email, "ProjectFlowAI");
    }
}

public record VerifyTwoFactorCommand(Guid UserId, string Code) : IRequest<Unit>;

public class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}

public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly ITotpService _totp;

    public VerifyTwoFactorCommandHandler(IProjectFlowDbContext db, ITotpService totp)
    {
        _db = db; _totp = totp;
    }

    public async Task<Unit> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        if (string.IsNullOrEmpty(user.TwoFactorSecret) || !_totp.ValidateCode(user.TwoFactorSecret, request.Code))
            throw new UnauthorizedDomainException("Invalid two-factor code.");

        user.TwoFactorEnabled = true;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------------------------
// OAuth
// ---------------------------------------------------------------------------------------------
public record GoogleLoginCommand(string IdToken, string? IpAddress) : IRequest<AuthResultDto>;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator() => RuleFor(x => x.IdToken).NotEmpty();
}

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IOAuthVerifier _oauth;
    private readonly ITokenService _tokens;
    private readonly IDateTimeProvider _clock;

    public GoogleLoginCommandHandler(IProjectFlowDbContext db, IOAuthVerifier oauth, ITokenService tokens, IDateTimeProvider clock)
    {
        _db = db; _oauth = oauth; _tokens = tokens; _clock = clock;
    }

    public async Task<AuthResultDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var info = await _oauth.VerifyGoogleIdTokenAsync(request.IdToken, cancellationToken);
        var user = await OAuthUpsertHelper.UpsertAsync(_db, info, isGoogle: true, _clock, cancellationToken);

        var (roles, permissions) = await UserAuthorizationHelper.GetRolesAndPermissionsAsync(_db, user.Id, cancellationToken);
        var access = _tokens.GenerateAccessToken(user, roles, permissions);
        var refreshRaw = _tokens.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, TokenHash = _tokens.HashToken(refreshRaw),
            ExpiresAt = _clock.UtcNow.AddDays(7), CreatedAt = _clock.UtcNow, CreatedByIp = request.IpAddress
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(access, refreshRaw, _clock.UtcNow.AddMinutes(30), RegisterCommandHandler.ToDto(user, roles, permissions));
    }
}

public record MicrosoftLoginCommand(string AccessToken, string? IpAddress) : IRequest<AuthResultDto>;

public class MicrosoftLoginCommandValidator : AbstractValidator<MicrosoftLoginCommand>
{
    public MicrosoftLoginCommandValidator() => RuleFor(x => x.AccessToken).NotEmpty();
}

public class MicrosoftLoginCommandHandler : IRequestHandler<MicrosoftLoginCommand, AuthResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IOAuthVerifier _oauth;
    private readonly ITokenService _tokens;
    private readonly IDateTimeProvider _clock;

    public MicrosoftLoginCommandHandler(IProjectFlowDbContext db, IOAuthVerifier oauth, ITokenService tokens, IDateTimeProvider clock)
    {
        _db = db; _oauth = oauth; _tokens = tokens; _clock = clock;
    }

    public async Task<AuthResultDto> Handle(MicrosoftLoginCommand request, CancellationToken cancellationToken)
    {
        var info = await _oauth.VerifyMicrosoftAccessTokenAsync(request.AccessToken, cancellationToken);
        var user = await OAuthUpsertHelper.UpsertAsync(_db, info, isGoogle: false, _clock, cancellationToken);

        var (roles, permissions) = await UserAuthorizationHelper.GetRolesAndPermissionsAsync(_db, user.Id, cancellationToken);
        var access = _tokens.GenerateAccessToken(user, roles, permissions);
        var refreshRaw = _tokens.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, TokenHash = _tokens.HashToken(refreshRaw),
            ExpiresAt = _clock.UtcNow.AddDays(7), CreatedAt = _clock.UtcNow, CreatedByIp = request.IpAddress
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(access, refreshRaw, _clock.UtcNow.AddMinutes(30), RegisterCommandHandler.ToDto(user, roles, permissions));
    }
}

internal static class OAuthUpsertHelper
{
    public static async Task<User> UpsertAsync(IProjectFlowDbContext db, OAuthUserInfo info, bool isGoogle,
        IDateTimeProvider clock, CancellationToken cancellationToken)
    {
        var normalizedEmail = info.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user != null)
        {
            if (isGoogle) user.GoogleId ??= info.ProviderId;
            else user.MicrosoftId ??= info.ProviderId;
            return user;
        }

        var nameParts = info.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        user = new User
        {
            Email = normalizedEmail,
            PasswordHash = string.Empty, // OAuth-only account — no local password set
            FirstName = nameParts.Length > 0 ? nameParts[0] : info.Name,
            LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
            IsEmailVerified = true,
            Status = UserStatus.Active,
            GoogleId = isGoogle ? info.ProviderId : null,
            MicrosoftId = isGoogle ? null : info.ProviderId,
            CreatedAt = clock.UtcNow
        };
        db.Users.Add(user);
        return user;
    }
}
