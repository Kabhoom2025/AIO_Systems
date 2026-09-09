using System.Text.Json;
using FlowSphere.Application.Apps.Common;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.SubmitGuestAppRecord;

public class SubmitGuestAppRecordCommandHandler : IRequestHandler<SubmitGuestAppRecordCommand, Result<SubmitGuestAppRecordResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentEnvironmentContext _currentEnvironment;
    private readonly ICaptchaService _captcha;
    private readonly IPasswordHasher _hasher;
    private readonly IAppNotificationDispatcher _notifications;
    private readonly IAppTriggerDispatcher _triggers;

    public SubmitGuestAppRecordCommandHandler(
        IApplicationDbContext db, ICurrentEnvironmentContext currentEnvironment, ICaptchaService captcha, IPasswordHasher hasher,
        IAppNotificationDispatcher notifications, IAppTriggerDispatcher triggers)
    {
        _db = db;
        _currentEnvironment = currentEnvironment;
        _captcha = captcha;
        _hasher = hasher;
        _notifications = notifications;
        _triggers = triggers;
    }

    public async Task<Result<SubmitGuestAppRecordResultDto>> Handle(SubmitGuestAppRecordCommand request, CancellationToken cancellationToken)
    {
        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Stage == _currentEnvironment.Stage && a.IsPublished, cancellationToken);

        if (app is null)
        {
            return Result<SubmitGuestAppRecordResultDto>.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        var (guestEnabled, guestToken) = AppSubmissionValidation.ReadSharedAccess(app.SettingsJson, "guest");
        if (!guestEnabled || string.IsNullOrEmpty(guestToken) || guestToken != request.GuestToken)
        {
            return Result<SubmitGuestAppRecordResultDto>.Failure(Error.Unauthorized("This app's guest link is disabled or the link is invalid."));
        }

        if (!IsValidJsonObject(request.DataJson))
        {
            return Result<SubmitGuestAppRecordResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["dataJson"] = new[] { "The submitted data must be a JSON object." },
            }));
        }

        if (AppSubmissionValidation.IsCaptchaRequiredOnSubmit(app.SettingsJson)
            && (request.CaptchaToken is null || request.CaptchaAnswer is null || !_captcha.Verify(request.CaptchaToken, request.CaptchaAnswer.Value)))
        {
            return Result<SubmitGuestAppRecordResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["captcha"] = new[] { "The captcha answer is missing or incorrect." },
            }));
        }

        if (AppSubmissionValidation.IsOtpRequiredOnSubmit(app.SettingsJson))
        {
            var otpMethod = AppSubmissionValidation.ReadOtpMethod(app.SettingsJson);
            var consumedChallenges = new List<AppOtpChallenge>();

            if (otpMethod is "Email" or "Both")
            {
                var emailMatch = await FindValidOtpChallengeAsync(app.Id, AppOtpChannel.Email, request.OtpEmail, request.OtpEmailCode, cancellationToken);
                if (emailMatch is null)
                {
                    return Result<SubmitGuestAppRecordResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
                    {
                        ["otp"] = new[] { "The email verification code is missing, invalid, or expired." },
                    }));
                }
                consumedChallenges.Add(emailMatch);
            }

            if (otpMethod is "Mobile" or "Both")
            {
                var phoneMatch = await FindValidOtpChallengeAsync(app.Id, AppOtpChannel.Mobile, request.OtpPhone, request.OtpPhoneCode, cancellationToken);
                if (phoneMatch is null)
                {
                    return Result<SubmitGuestAppRecordResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
                    {
                        ["otp"] = new[] { "The mobile verification code is missing, invalid, or expired." },
                    }));
                }
                consumedChallenges.Add(phoneMatch);
            }

            consumedChallenges.ForEach(c => c.Consumed = true);
        }

        var existingDataJsons = await _db.AppRecords
            .Where(r => r.AppDefinitionId == app.Id)
            .Select(r => r.DataJson)
            .ToListAsync(cancellationToken);
        var uniqueRecordCheck = AppSubmissionValidation.CheckUniqueRecord(app.SettingsJson, request.DataJson, existingDataJsons);
        if (uniqueRecordCheck.Restricted)
        {
            return Result<SubmitGuestAppRecordResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["uniqueRecord"] = new[] { uniqueRecordCheck.Message ?? "This record duplicates an existing one." },
            }));
        }

        var record = AppRecord.Create(app.Id, request.DataJson, null, _currentEnvironment.Stage, AppRecordSource.Guest);
        _db.AppRecords.Add(record);

        var fieldMappingJson = GetSectionFieldMappingJson(app.FormSchemaJson, request.SectionId) ?? app.FieldMappingJson;
        if (app.LinkedTableId is not null)
        {
            var mapped = MapFields(request.DataJson, fieldMappingJson);
            _db.TableRecords.Add(TableRecord.Create(app.LinkedTableId.Value, mapped, 0, _currentEnvironment.Stage));
        }

        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _notifications.DispatchRecordSubmittedAsync(app.Workspace.OrganizationId, app.Id, request.DataJson, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Notification failures must never fail an otherwise successful guest submission.
        }

        try
        {
            await _triggers.DispatchRecordSubmittedAsync(app.Workspace.OrganizationId, app.Id, _currentEnvironment.Stage, request.DataJson, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A misconfigured trigger must never fail an otherwise successful guest submission.
        }

        return Result<SubmitGuestAppRecordResultDto>.Success(
            new SubmitGuestAppRecordResultDto(record.Id, uniqueRecordCheck.Warned, uniqueRecordCheck.Message));
    }

    private async Task<AppOtpChallenge?> FindValidOtpChallengeAsync(int appId, AppOtpChannel channel, string? recipient, string? code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipient) || string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var candidates = await _db.AppOtpChallenges
            .Where(c => c.AppId == appId && c.Channel == channel && c.Recipient == recipient && !c.Consumed && c.ExpiresAt > now)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault(c => _hasher.Verify(code, c.CodeHash));
    }

    private static string? GetSectionFieldMappingJson(string formSchemaJson, string? sectionId)
    {
        if (string.IsNullOrEmpty(sectionId))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(formSchemaJson);
            if (!doc.RootElement.TryGetProperty("sections", out var sections) || sections.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var section in sections.EnumerateArray())
            {
                if (section.TryGetProperty("id", out var idEl) && idEl.GetString() == sectionId
                    && section.TryGetProperty("fieldMappingJson", out var mapEl) && mapEl.ValueKind == JsonValueKind.String)
                {
                    return mapEl.GetString();
                }
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool IsValidJsonObject(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string MapFields(string dataJson, string fieldMappingJson)
    {
        var data = System.Text.Json.Nodes.JsonNode.Parse(dataJson)?.AsObject() ?? new System.Text.Json.Nodes.JsonObject();
        var mapping = System.Text.Json.Nodes.JsonNode.Parse(fieldMappingJson)?.AsObject() ?? new System.Text.Json.Nodes.JsonObject();

        var mapped = new System.Text.Json.Nodes.JsonObject();
        foreach (var (formFieldKey, tableColumnKeyNode) in mapping)
        {
            var tableColumnKey = tableColumnKeyNode?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(tableColumnKey))
            {
                continue;
            }

            mapped[tableColumnKey] = data.TryGetPropertyValue(formFieldKey, out var value) ? value?.DeepClone() : null;
        }

        return mapped.ToJsonString();
    }
}
