using Platform.Domain.Enums;

namespace Platform.Application.MappingDesigner;

public record MappingDto(
    Guid Id,
    Guid ApplicationId,
    Guid SourceComponentId,
    string SourceField,
    Guid ApiId,
    string ApiField,
    Guid? ServiceId,
    string? ServiceField,
    Guid ColumnId,
    TransformationType Transformation,
    string? TransformationConfigJson,
    DateTimeOffset CreatedAt);

public record CreateMappingRequest(
    Guid SourceComponentId,
    string SourceField,
    Guid ApiId,
    string ApiField,
    Guid? ServiceId,
    string? ServiceField,
    Guid ColumnId,
    TransformationType Transformation,
    string? TransformationConfigJson);

public record UpdateMappingRequest(
    string SourceField,
    string ApiField,
    string? ServiceField,
    TransformationType Transformation,
    string? TransformationConfigJson);
