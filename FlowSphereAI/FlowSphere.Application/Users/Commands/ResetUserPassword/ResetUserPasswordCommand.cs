using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Users.Commands.ResetUserPassword;

public record ResetUserPasswordCommand(int UserId, string NewPassword) : IRequest<Result>;
