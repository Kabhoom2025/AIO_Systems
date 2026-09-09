using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Result>;
