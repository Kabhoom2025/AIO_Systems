using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.VerifyAppOtp;

/// <summary>Deliberately NOT IAppScopedRequest - see RequestAppOtpCommand.</summary>
public record VerifyAppOtpCommand(int AppId, AppOtpChannel Channel, string Recipient, string Code) : IRequest<Result>;
