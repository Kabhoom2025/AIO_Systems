using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Auth.Commands.Register;

public record RegisterCommand(string OrganizationName, string AdminName, string Email, string Password)
    : IRequest<Result<RegisterResult>>;

public record RegisterResult(
    string Token,
    string Name,
    string Email,
    int OrganizationId,
    string[] Permissions,
    bool IsEmailVerified,
    string VerificationLink,
    string RoleName);
