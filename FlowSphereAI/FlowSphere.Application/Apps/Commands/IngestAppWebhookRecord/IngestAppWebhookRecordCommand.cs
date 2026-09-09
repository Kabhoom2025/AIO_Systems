using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.IngestAppWebhookRecord;

/// <summary>Machine-to-machine counterpart to SubmitAppCommand for apps with Shared Settings >
/// Webhook enabled. Deliberately NOT IAppScopedRequest - authenticated by ApiKey (matched against
/// SettingsJson.shared.webhookApiKey) instead of a JWT. No Captcha/OTP checks (those are
/// human-verification gates, meaningless for a server-to-server call) but Unique Record still
/// applies - it's a data-integrity rule, not a human check.</summary>
public record IngestAppWebhookRecordCommand(int AppId, string ApiKey, string DataJson) : IRequest<Result<IngestAppWebhookRecordResultDto>>;

public record IngestAppWebhookRecordResultDto(int RecordId);
