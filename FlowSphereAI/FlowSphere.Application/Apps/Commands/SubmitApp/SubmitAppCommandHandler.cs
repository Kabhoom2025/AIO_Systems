using System.Text.Json;
using System.Text.Json.Nodes;
using FlowSphere.Application.Apps.Common;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.SubmitApp;

public class SubmitAppCommandHandler : IRequestHandler<SubmitAppCommand, Result<SubmitAppResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;
    private readonly IExecutionQueue _queue;
    private readonly ICaptchaService _captcha;
    private readonly IPasswordHasher _hasher;
    private readonly IAppNotificationDispatcher _notifications;
    private readonly IAppTriggerDispatcher _triggers;

    public SubmitAppCommandHandler(
        IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment, IExecutionQueue queue,
        ICaptchaService captcha, IPasswordHasher hasher, IAppNotificationDispatcher notifications, IAppTriggerDispatcher triggers)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
        _queue = queue;
        _captcha = captcha;
        _hasher = hasher;
        _notifications = notifications;
        _triggers = triggers;
    }

    public async Task<Result<SubmitAppResultDto>> Handle(SubmitAppCommand request, CancellationToken cancellationToken)
    {
        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);

        if (app is null)
        {
            return Result<SubmitAppResultDto>.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        if (!IsValidJsonObject(request.DataJson))
        {
            return Result<SubmitAppResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["dataJson"] = new[] { "The submitted data must be a JSON object." },
            }));
        }

        if (!request.IsTest && !app.IsPublished)
        {
            return Result<SubmitAppResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["app"] = new[] { "This app must be published before it can be launched." },
            }));
        }

        AppSubmissionValidation.UniqueRecordCheck? uniqueRecordCheck = null;
        if (!request.IsTest)
        {
            if (AppSubmissionValidation.IsCaptchaRequiredOnSubmit(app.SettingsJson)
                && (request.CaptchaToken is null || request.CaptchaAnswer is null || !_captcha.Verify(request.CaptchaToken, request.CaptchaAnswer.Value)))
            {
                return Result<SubmitAppResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
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
                        return Result<SubmitAppResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
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
                        return Result<SubmitAppResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
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
            uniqueRecordCheck = AppSubmissionValidation.CheckUniqueRecord(app.SettingsJson, request.DataJson, existingDataJsons);
            if (uniqueRecordCheck.Restricted)
            {
                return Result<SubmitAppResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
                {
                    ["uniqueRecord"] = new[] { uniqueRecordCheck.Message ?? "This record duplicates an existing one." },
                }));
            }
        }

        var (moduleWorkflowId, moduleTableId, moduleFieldMappingJson) = GetModuleLinks(app.FormSchemaJson, request.SectionId);
        var effectiveWorkflowId = moduleWorkflowId ?? app.LinkedWorkflowDefinitionId;
        var effectiveTableId = moduleTableId ?? app.LinkedTableId;
        var effectiveFieldMappingJson = moduleTableId is not null ? moduleFieldMappingJson : app.FieldMappingJson;

        string? mappedTableDataJson = null;
        if (effectiveTableId is not null)
        {
            mappedTableDataJson = MapFields(request.DataJson, effectiveFieldMappingJson);
        }

        WorkflowExecution? execution = null;
        if (effectiveWorkflowId is not null)
        {
            var deployedVersionId = await _db.WorkflowStageDeployments
                .Where(d => d.WorkflowDefinitionId == effectiveWorkflowId && d.Stage == _currentEnvironment.Stage)
                .Select(d => (int?)d.WorkflowVersionId)
                .FirstOrDefaultAsync(cancellationToken);

            var version = deployedVersionId is null
                ? null
                : await _db.WorkflowVersions.FirstOrDefaultAsync(v => v.Id == deployedVersionId, cancellationToken);

            if (version is null)
            {
                return Result<SubmitAppResultDto>.Failure(Error.NotFound($"This app's linked workflow has no version deployed to the {_currentEnvironment.Stage} stage."));
            }

            var organization = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == _currentUser.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<SubmitAppResultDto>.Failure(Error.NotFound("Organization not found."));
            }

            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var executionsThisMonth = await _db.WorkflowExecutions
                .CountAsync(e => e.CreatedDate >= monthStart, cancellationToken);

            if (executionsThisMonth >= organization.MonthlyExecutionQuota)
            {
                return Result<SubmitAppResultDto>.Failure(Error.QuotaExceeded(
                    $"Monthly execution quota of {organization.MonthlyExecutionQuota} reached for the {organization.PlanTier} plan."));
            }

            execution = WorkflowExecution.CreateQueued(
                _currentUser.OrganizationId, version, request.DataJson, request.IsTest ? "AppTest" : "AppLaunch", _currentEnvironment.Stage);
            _db.WorkflowExecutions.Add(execution);
        }

        int? recordId = null;
        if (!request.IsTest)
        {
            var record = AppRecord.Create(app.Id, request.DataJson, _currentUser.UserId, _currentEnvironment.Stage);
            _db.AppRecords.Add(record);

            if (effectiveTableId is not null && mappedTableDataJson is not null)
            {
                _db.TableRecords.Add(TableRecord.Create(effectiveTableId.Value, mappedTableDataJson, _currentUser.UserId, _currentEnvironment.Stage));
            }

            await _db.SaveChangesAsync(cancellationToken);
            recordId = record.Id;

            try
            {
                await _notifications.DispatchRecordSubmittedAsync(_currentUser.OrganizationId, app.Id, request.DataJson, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A notification-rule bug or a transient SMTP issue must never fail an otherwise
                // successful submission - the record is already persisted.
            }

            try
            {
                await _triggers.DispatchRecordSubmittedAsync(_currentUser.OrganizationId, app.Id, _currentEnvironment.Stage, request.DataJson, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A misconfigured trigger must never fail an otherwise successful submission.
            }
        }
        else if (execution is not null)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        if (execution is not null)
        {
            await _queue.EnqueueAsync(execution.Id, cancellationToken);
        }

        return Result<SubmitAppResultDto>.Success(new SubmitAppResultDto(
            recordId, mappedTableDataJson, execution?.Id, request.IsTest,
            uniqueRecordCheck?.Warned ?? false, uniqueRecordCheck?.Message));
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

    /// <summary>Reads a module's (section's) own workflowDefinitionId/linkedTableId/
    /// fieldMappingJson out of FormSchemaJson's { "sections": [...] } shape (see formSchema.ts on
    /// the client) - each returns null when sectionId is absent or the section has no override,
    /// letting the caller fall back to the app-level link for backward compatibility.</summary>
    private static (int? WorkflowDefinitionId, int? LinkedTableId, string FieldMappingJson) GetModuleLinks(
        string formSchemaJson, string? sectionId)
    {
        if (string.IsNullOrEmpty(sectionId))
        {
            return (null, null, "{}");
        }

        try
        {
            using var doc = JsonDocument.Parse(formSchemaJson);
            if (!doc.RootElement.TryGetProperty("sections", out var sectionsEl) || sectionsEl.ValueKind != JsonValueKind.Array)
            {
                return (null, null, "{}");
            }

            foreach (var section in sectionsEl.EnumerateArray())
            {
                var id = section.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String ? idEl.GetString() : null;
                if (id != sectionId)
                {
                    continue;
                }

                int? workflowId = section.TryGetProperty("workflowDefinitionId", out var wfEl) && wfEl.ValueKind == JsonValueKind.Number
                    ? wfEl.GetInt32()
                    : null;
                int? tableId = section.TryGetProperty("linkedTableId", out var tableEl) && tableEl.ValueKind == JsonValueKind.Number
                    ? tableEl.GetInt32()
                    : null;
                var fieldMappingJson = section.TryGetProperty("fieldMappingJson", out var mapEl) && mapEl.ValueKind == JsonValueKind.String
                    ? mapEl.GetString() ?? "{}"
                    : "{}";

                return (workflowId, tableId, fieldMappingJson);
            }

            return (null, null, "{}");
        }
        catch (JsonException)
        {
            return (null, null, "{}");
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
        var data = JsonNode.Parse(dataJson)?.AsObject() ?? new JsonObject();
        var mapping = JsonNode.Parse(fieldMappingJson)?.AsObject() ?? new JsonObject();

        var mapped = new JsonObject();
        foreach (var (formFieldKey, tableColumnKeyNode) in mapping)
        {
            var tableColumnKey = tableColumnKeyNode?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(tableColumnKey))
            {
                continue;
            }

            mapped[tableColumnKey] = data.TryGetPropertyValue(formFieldKey, out var value)
                ? value?.DeepClone()
                : null;
        }

        return mapped.ToJsonString();
    }
}
