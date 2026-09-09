using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Users.Commands.ChangeUserRole;

public record ChangeUserRoleCommand(int UserId, string RoleName) : IRequest<Result>;
