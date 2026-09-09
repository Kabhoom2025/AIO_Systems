using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Infrastructure.Notifications;

public class AppTriggerDispatcher : IAppTriggerDispatcher
{
    private readonly IApplicationDbContext _db;
    private readonly IExecutionQueue _queue;

    public AppTriggerDispatcher(IApplicationDbContext db, IExecutionQueue queue)
    {
        _db = db;
        _queue = queue;
    }

    public async Task DispatchRecordSubmittedAsync(int organizationId, int sourceAppId, EnvironmentStage stage, string dataJson, CancellationToken cancellationToken)
    {
        var triggers = await _db.AppTriggers
            .Where(t => t.SourceAppId == sourceAppId && t.OrganizationId == organizationId && t.IsEnabled)
            .ToListAsync(cancellationToken);

        if (triggers.Count == 0)
        {
            return;
        }

        foreach (var trigger in triggers)
        {
            var destinationApp = await _db.AppDefinitions
                .FirstOrDefaultAsync(a => a.Id == trigger.DestinationAppId && a.Stage == stage, cancellationToken);
            if (destinationApp is null)
            {
                continue;
            }

            var mappedDataJson = MapFields(dataJson, trigger.FieldMappingJson);
            var record = AppRecord.Create(destinationApp.Id, mappedDataJson, null, stage, AppRecordSource.Trigger);
            _db.AppRecords.Add(record);
            await _db.SaveChangesAsync(cancellationToken);

            if (trigger.ActionType == AppTriggerActionType.CreateTask && destinationApp.LinkedWorkflowDefinitionId is not null)
            {
                await StartDestinationWorkflowAsync(organizationId, destinationApp.LinkedWorkflowDefinitionId.Value, stage, mappedDataJson, cancellationToken);
            }
        }
    }

    private async Task StartDestinationWorkflowAsync(int organizationId, int workflowDefinitionId, EnvironmentStage stage, string dataJson, CancellationToken cancellationToken)
    {
        var deployedVersionId = await _db.WorkflowStageDeployments
            .Where(d => d.WorkflowDefinitionId == workflowDefinitionId && d.Stage == stage)
            .Select(d => (int?)d.WorkflowVersionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (deployedVersionId is null)
        {
            return;
        }

        var version = await _db.WorkflowVersions.FirstOrDefaultAsync(v => v.Id == deployedVersionId, cancellationToken);
        if (version is null)
        {
            return;
        }

        var execution = WorkflowExecution.CreateQueued(organizationId, version, dataJson, "AppTrigger", stage);
        _db.WorkflowExecutions.Add(execution);
        await _db.SaveChangesAsync(cancellationToken);

        await _queue.EnqueueAsync(execution.Id, cancellationToken);
    }

    private static string MapFields(string dataJson, string fieldMappingJson)
    {
        var data = JsonNode.Parse(dataJson)?.AsObject() ?? new JsonObject();
        var mapping = JsonNode.Parse(fieldMappingJson)?.AsObject() ?? new JsonObject();

        var mapped = new JsonObject();
        foreach (var (sourceFieldKey, destinationFieldKeyNode) in mapping)
        {
            var destinationFieldKey = destinationFieldKeyNode?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(destinationFieldKey))
            {
                continue;
            }

            mapped[destinationFieldKey] = data.TryGetPropertyValue(sourceFieldKey, out var value) ? value?.DeepClone() : null;
        }

        return mapped.ToJsonString();
    }
}
