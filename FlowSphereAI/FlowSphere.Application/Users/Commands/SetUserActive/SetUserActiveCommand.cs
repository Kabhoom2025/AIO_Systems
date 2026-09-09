using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Users.Commands.SetUserActive;

public record SetUserActiveCommand(int UserId, bool IsActive) : IRequest<Result>;
