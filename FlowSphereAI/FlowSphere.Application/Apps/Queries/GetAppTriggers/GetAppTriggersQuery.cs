using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetAppTriggers;

public record GetAppTriggersQuery(int SourceAppId) : IRequest<Result<List<AppTriggerDto>>>;

public record AppTriggerDto(
    int Id, string Name, int DestinationAppId, string DestinationAppName, AppTriggerActionType ActionType, string FieldMappingJson, bool IsEnabled);
