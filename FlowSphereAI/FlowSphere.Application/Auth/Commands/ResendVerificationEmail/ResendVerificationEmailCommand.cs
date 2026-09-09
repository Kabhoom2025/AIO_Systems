using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Auth.Commands.ResendVerificationEmail;

public record ResendVerificationEmailCommand : IRequest<Result>;
