using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Users.Commands.InviteUser;

/// <summary>When RoleName is omitted, the created user is left Pending (no role, unverified
/// email) until an admin confirms their email and assigns a role - matching Quixy's two-condition
/// activation. Supplying RoleName keeps the older immediate-activation behavior for any direct
/// API caller that still relies on it.</summary>
public record InviteUserCommand(string Name, string Email, string Password, int? ManagerId = null, string? RoleName = null)
    : IRequest<Result<InviteUserResult>>;

public record InviteUserResult(int UserId, string Name, string Email, string? RoleName, bool IsEmailVerified);
