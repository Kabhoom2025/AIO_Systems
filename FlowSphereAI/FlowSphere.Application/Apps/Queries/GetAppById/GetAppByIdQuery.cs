using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetAppById;

public record GetAppByIdQuery(int Id) : IRequest<Result<AppDetailDto>>;

public record AppDetailDto(
    int Id,
    int WorkspaceId,
    string Name,
    string? Description,
    string? Icon,
    string FormSchemaJson,
    int? LinkedWorkflowDefinitionId,
    int? LinkedTableId,
    string FieldMappingJson,
    string BusinessRulesJson,
    string AccessPermissionsJson,
    bool IsPublished,
    DateTime? PublishedAt,
    DateTime CreatedDate,
    string SettingsJson,
    string? UserManualMarkdown,
    string? PublishedSnapshotJson);
