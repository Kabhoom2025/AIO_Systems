using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : IRequest<Result>;
