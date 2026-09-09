using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResult>>;

public record LoginResult(string Token, string Name, string Email, int OrganizationId, string[] Permissions, bool IsEmailVerified, string RoleName);
