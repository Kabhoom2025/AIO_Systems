using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Invitations;

public record InviteUserCommand(Guid OrganizationId, string Email, Guid RoleId, Guid InvitedByUserId) : IRequest<InvitationDto>;

public class InviteUserCommandValidator : AbstractValidator<InviteUserCommand>
{
    public InviteUserCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.InvitedByUserId).NotEmpty();
    }
}

public class InviteUserCommandHandler : IRequestHandler<InviteUserCommand, InvitationDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokens;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailSender _email;

    public InviteUserCommandHandler(IProjectFlowDbContext db, IMapper mapper, ITokenService tokens,
        IDateTimeProvider clock, IEmailSender email)
    {
        _db = db; _mapper = mapper; _tokens = tokens; _clock = clock; _email = email;
    }

    public async Task<InvitationDto> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Roles.AnyAsync(r => r.Id == request.RoleId, cancellationToken))
            throw new NotFoundException("Role", request.RoleId);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var raw = _tokens.GenerateRefreshToken();
        var invitation = new Invitation
        {
            OrganizationId = request.OrganizationId,
            Email = normalizedEmail,
            RoleId = request.RoleId,
            InvitedByUserId = request.InvitedByUserId,
            TokenHash = _tokens.HashToken(raw),
            Status = InvitationStatus.Pending,
            ExpiresAt = _clock.UtcNow.AddDays(7),
            CreatedAt = _clock.UtcNow
        };
        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync(cancellationToken);

        await _email.SendAsync(normalizedEmail, "You've been invited to ProjectFlow AI",
            $"You have been invited to join a ProjectFlow AI organization. Your invitation code is: {raw}", cancellationToken);

        var withRole = await _db.Invitations.Include(i => i.Role).FirstAsync(i => i.Id == invitation.Id, cancellationToken);
        return _mapper.Map<InvitationDto>(withRole);
    }
}

public record AcceptInvitationCommand(string Email, string Token, string Password, string FirstName, string LastName) : IRequest<AuthResultDtoRef>;

/// <summary>Kept as a thin alias to Application.DTOs.AuthResultDto to avoid a circular namespace reference in this file.</summary>
public record AuthResultDtoRef(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, UserDto User);

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).Matches("[A-Z]").Matches("[a-z]").Matches("[0-9]");
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, AuthResultDtoRef>
{
    private readonly IProjectFlowDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IPasswordHasher _hasher;
    private readonly IDateTimeProvider _clock;

    public AcceptInvitationCommandHandler(IProjectFlowDbContext db, ITokenService tokens, IPasswordHasher hasher, IDateTimeProvider clock)
    {
        _db = db; _tokens = tokens; _hasher = hasher; _clock = clock;
    }

    public async Task<AuthResultDtoRef> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var hash = _tokens.HashToken(request.Token);
        var invitation = await _db.Invitations.FirstOrDefaultAsync(
            i => i.Email == normalizedEmail && i.TokenHash == hash, cancellationToken);

        if (invitation == null || invitation.Status != InvitationStatus.Pending || invitation.ExpiresAt < _clock.UtcNow)
            throw new UnauthorizedDomainException("Invalid or expired invitation.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user == null)
        {
            user = new User
            {
                Email = normalizedEmail,
                PasswordHash = _hasher.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Status = UserStatus.Active,
                IsEmailVerified = true,
                OrganizationId = invitation.OrganizationId,
                CreatedAt = _clock.UtcNow
            };
            _db.Users.Add(user);
        }
        else
        {
            user.OrganizationId = invitation.OrganizationId;
            user.Status = UserStatus.Active;
        }

        invitation.Status = InvitationStatus.Accepted;
        await _db.SaveChangesAsync(cancellationToken);

        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = invitation.RoleId, OrganizationId = invitation.OrganizationId });
        await _db.SaveChangesAsync(cancellationToken);

        var (roles, permissions) = await Common.UserAuthorizationHelper.GetRolesAndPermissionsAsync(_db, user.Id, cancellationToken);
        var access = _tokens.GenerateAccessToken(user, roles, permissions);
        var refreshRaw = _tokens.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, TokenHash = _tokens.HashToken(refreshRaw),
            ExpiresAt = _clock.UtcNow.AddDays(7), CreatedAt = _clock.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.AvatarUrl,
            user.IsEmailVerified, user.TwoFactorEnabled, user.Status, user.LastLoginAt, user.OrganizationId, user.CreatedAt,
            roles, permissions);
        return new AuthResultDtoRef(access, refreshRaw, _clock.UtcNow.AddMinutes(30), userDto);
    }
}

public record RevokeInvitationCommand(Guid Id) : IRequest<Unit>;

public class RevokeInvitationCommandValidator : AbstractValidator<RevokeInvitationCommand>
{
    public RevokeInvitationCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class RevokeInvitationCommandHandler : IRequestHandler<RevokeInvitationCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public RevokeInvitationCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _db.Invitations.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.Id);

        invitation.Status = InvitationStatus.Revoked;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record ListInvitationsQuery(Guid OrganizationId, InvitationStatus? Status = null, int Page = 1, int PageSize = 20,
    string? SortBy = null, string? SortDir = "asc") : IRequest<PagedResult<InvitationDto>>;

public class ListInvitationsQueryHandler : IRequestHandler<ListInvitationsQuery, PagedResult<InvitationDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListInvitationsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<InvitationDto>> Handle(ListInvitationsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Invitations.Include(i => i.Role).Where(i => i.OrganizationId == request.OrganizationId);
        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);
        var sorted = query.ApplySort(request.SortBy, request.SortDir, nameof(Invitation.CreatedAt));

        var items = await sorted.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ProjectTo<InvitationDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

        return new PagedResult<InvitationDto>(items, total, request.Page, request.PageSize);
    }
}
