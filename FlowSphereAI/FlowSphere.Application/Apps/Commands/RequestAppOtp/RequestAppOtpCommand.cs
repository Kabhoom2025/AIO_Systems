using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.RequestAppOtp;

/// <summary>Deliberately NOT IAppScopedRequest - this is called anonymously from the public
/// Launch page (Accessibility &gt; OTP Validation), before the caller has any org/user context,
/// so it must skip WorkspacePermissionBehavior entirely. AppsController exposes it via
/// [AllowAnonymous]. Recipient is an email address (Channel=Email) or phone number
/// (Channel=Mobile) - "Mobile & Email" mode means the client calls this once per channel.</summary>
public record RequestAppOtpCommand(int AppId, AppOtpChannel Channel, string Recipient) : IRequest<Result>;
