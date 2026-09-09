using Platform.Domain.Enums;

namespace Platform.Application.ApiDesigner;

public record ServiceDto(
    Guid Id,
    Guid ApplicationId,
    string Name,
    Guid? TableId,
    string? Description,
    DateTimeOffset CreatedAt);

public record CreateServiceRequest(string Name, Guid? TableId, string? Description);

public record UpdateServiceRequest(string Name, Guid? TableId, string? Description);

public record ApiEndpointDto(
    Guid Id,
    Guid ApplicationId,
    ApiHttpMethod Method,
    string Path,
    string? RequestSchemaJson,
    string? ResponseSchemaJson,
    Guid? ServiceId,
    Guid? TableId,
    DateTimeOffset CreatedAt);

public record CreateApiEndpointRequest(
    ApiHttpMethod Method,
    string Path,
    string? RequestSchemaJson,
    string? ResponseSchemaJson,
    Guid? ServiceId,
    Guid? TableId);

public record UpdateApiEndpointRequest(
    ApiHttpMethod Method,
    string Path,
    string? RequestSchemaJson,
    string? ResponseSchemaJson,
    Guid? ServiceId,
    Guid? TableId);
