using System.Text.Json;
using FlowSphere.Application.Apps.Common;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.IngestAppWebhookRecord;

public class IngestAppWebhookRecordCommandHandler : IRequestHandler<IngestAppWebhookRecordCommand, Result<IngestAppWebhookRecordResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentEnvironmentContext _currentEnvironment;
    private readonly IAppNotificationDispatcher _notifications;

    public IngestAppWebhookRecordCommandHandler(IApplicationDbContext db, ICurrentEnvironmentContext currentEnvironment, IAppNotificationDispatcher notifications)
    {
        _db = db;
        _currentEnvironment = currentEnvironment;
        _notifications = notifications;
    }

    public async Task<Result<IngestAppWebhookRecordResultDto>> Handle(IngestAppWebhookRecordCommand request, CancellationToken cancellationToken)
    {
        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Stage == _currentEnvironment.Stage && a.IsPublished, cancellationToken);

        if (app is null)
        {
            return Result<IngestAppWebhookRecordResultDto>.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        var (webhookEnabled, apiKey) = AppSubmissionValidation.ReadSharedAccess(app.SettingsJson, "webhook");
        if (!webhookEnabled || string.IsNullOrEmpty(apiKey) || apiKey != request.ApiKey)
        {
            return Result<IngestAppWebhookRecordResultDto>.Failure(Error.Unauthorized("This app's webhook is disabled or the API key is invalid."));
        }

        if (!IsValidJsonObject(request.DataJson))
        {
            return Result<IngestAppWebhookRecordResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["dataJson"] = new[] { "The submitted data must be a JSON object." },
            }));
        }

        var existingDataJsons = await _db.AppRecords
            .Where(r => r.AppDefinitionId == app.Id)
            .Select(r => r.DataJson)
            .ToListAsync(cancellationToken);
        var uniqueRecordCheck = AppSubmissionValidation.CheckUniqueRecord(app.SettingsJson, request.DataJson, existingDataJsons);
        if (uniqueRecordCheck.Restricted)
        {
            return Result<IngestAppWebhookRecordResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["uniqueRecord"] = new[] { uniqueRecordCheck.Message ?? "This record duplicates an existing one." },
            }));
        }

        var record = AppRecord.Create(app.Id, request.DataJson, null, _currentEnvironment.Stage, AppRecordSource.Webhook);
        _db.AppRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _notifications.DispatchRecordSubmittedAsync(app.Workspace.OrganizationId, app.Id, request.DataJson, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Notification failures must never fail an otherwise successful webhook ingest.
        }

        return Result<IngestAppWebhookRecordResultDto>.Success(new IngestAppWebhookRecordResultDto(record.Id));
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
}
