using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.SubmitGuestAppRecord;

/// <summary>Anonymous counterpart to SubmitAppCommand for apps with Shared Settings > Guest App
/// Link enabled - deliberately NOT IAppScopedRequest (there is no authenticated caller to check a
/// permission against). Scope is intentionally narrower than the authenticated path: it creates
/// an AppRecord (and a mapped TableRecord if one is linked) and dispatches notifications, but does
/// not trigger a linked workflow execution - guest links are for public data collection, not
/// approval routing, which always requires an accountable submitter.</summary>
public record SubmitGuestAppRecordCommand(
    int AppId, string GuestToken, string DataJson, string? SectionId = null,
    string? CaptchaToken = null, int? CaptchaAnswer = null,
    string? OtpEmail = null, string? OtpEmailCode = null, string? OtpPhone = null, string? OtpPhoneCode = null)
    : IRequest<Result<SubmitGuestAppRecordResultDto>>;

public record SubmitGuestAppRecordResultDto(int RecordId, bool UniqueRecordWarning, string? UniqueRecordWarningMessage);
