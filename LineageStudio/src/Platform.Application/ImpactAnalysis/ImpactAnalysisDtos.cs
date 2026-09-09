namespace Platform.Application.ImpactAnalysis;

public record ScreenRefDto(Guid Id, string Name, string Route);

public record ApiRefDto(Guid Id, string Method, string Path);

public record ServiceRefDto(Guid Id, string Name);

public record TableRefDto(Guid Id, string Name);

public record ColumnRefDto(Guid Id, string Name, Guid TableId, string TableName);

public record ComponentRefDto(Guid Id, string Name, Guid ScreenId, string ScreenName);

public record ApiFieldRefDto(Guid ApiId, string Method, string Path, string ApiField);

public record ServiceFieldRefDto(Guid ServiceId, string ServiceName, string ServiceField);

/// <summary>Everything that touches a table, so removing/renaming it or changing its shape can
/// be judged for blast radius before doing it: which screens exercise it (transitively, via a
/// mapping into one of its columns), which APIs and services read/write it, and which other
/// tables are linked to it by foreign key in either direction.</summary>
public record TableImpactDto(
    Guid TableId,
    string TableName,
    IReadOnlyList<ScreenRefDto> Screens,
    IReadOnlyList<ApiRefDto> Apis,
    IReadOnlyList<ServiceRefDto> Services,
    IReadOnlyList<TableRefDto> ReferencingTables,
    IReadOnlyList<TableRefDto> ReferencedTables);

/// <summary>Same idea at column granularity: which UI fields/API fields/service fields flow into
/// this exact column, and which other columns hold a foreign key pointing at it (so changing its
/// type or dropping it would break those too).</summary>
public record ColumnImpactDto(
    Guid ColumnId,
    string ColumnName,
    Guid TableId,
    string TableName,
    IReadOnlyList<ComponentRefDto> Components,
    IReadOnlyList<ApiFieldRefDto> ApiFields,
    IReadOnlyList<ServiceFieldRefDto> ServiceFields,
    IReadOnlyList<ColumnRefDto> DependentColumns);
