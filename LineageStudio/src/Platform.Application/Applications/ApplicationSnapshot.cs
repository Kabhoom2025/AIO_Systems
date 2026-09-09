namespace Platform.Application.Applications;

/// <summary>
/// Full design-time configuration of an application at the moment it was published, serialized
/// into ApplicationVersion.SnapshotJson. Historical lineage executions reference a VersionId and
/// resolve node metadata (screen names, column types, mapping transformations, etc.) from this
/// snapshot rather than the live, possibly-since-edited platform.* rows, so reopening an old
/// execution always shows what actually ran.
/// </summary>
public record ApplicationSnapshot(
    Guid ApplicationId,
    string Name,
    string? Description,
    int VersionNumber,
    IReadOnlyList<ScreenSnapshot> Screens,
    IReadOnlyList<TableSnapshot> Tables,
    IReadOnlyList<ApiSnapshot> Apis,
    IReadOnlyList<ServiceSnapshot> Services,
    IReadOnlyList<MappingSnapshot> Mappings);

public record ScreenSnapshot(
    Guid Id,
    string Name,
    string Route,
    IReadOnlyList<ComponentSnapshot> Components);

public record ComponentSnapshot(
    Guid Id,
    string Type,
    string Name,
    double PositionX,
    double PositionY,
    string? PropertiesJson,
    string? ValidationJson,
    string? DataBinding,
    string? EventsJson);

public record TableSnapshot(
    Guid Id,
    string Name,
    string SchemaName,
    IReadOnlyList<ColumnSnapshot> Columns);

public record ColumnSnapshot(
    Guid Id,
    string Name,
    string DataType,
    int? Length,
    bool IsPrimaryKey,
    bool IsForeignKey,
    Guid? ReferencesTableId,
    Guid? ReferencesColumnId,
    bool IsUnique,
    bool IsIndexed,
    bool IsNullable,
    string? DefaultValue);

public record ApiSnapshot(
    Guid Id,
    string Method,
    string Path,
    string? RequestSchemaJson,
    string? ResponseSchemaJson,
    Guid? ServiceId,
    Guid? TableId);

public record ServiceSnapshot(
    Guid Id,
    string Name,
    Guid? TableId,
    string? Description);

public record MappingSnapshot(
    Guid Id,
    Guid SourceComponentId,
    string SourceField,
    Guid ApiId,
    string ApiField,
    Guid? ServiceId,
    string? ServiceField,
    Guid ColumnId,
    string Transformation,
    string? TransformationConfigJson);
