using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Users.Commands.ConfirmUserEmail;

/// <summary>Admin-driven email confirmation - mirrors Quixy's "admin confirms mail" action for a
/// Pending user, bypassing the token check since the admin is vouching directly (contrast with
/// the token-based self-service VerifyEmailCommand).</summary>
public record ConfirmUserEmailCommand(int UserId) : IRequest<Result>;
