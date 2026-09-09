using Platform.Domain.Enums;

namespace Platform.Application.UiBuilder;

public record ScreenDto(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string Route,
    DateTimeOffset CreatedAt);

public record CreateScreenRequest(string Name, string Route);

public record UpdateScreenRequest(string Name, string Route);

public record ComponentDto(
    Guid Id,
    Guid ScreenId,
    ComponentType Type,
    string Name,
    double PositionX,
    double PositionY,
    string? PropertiesJson,
    string? ValidationJson,
    string? DataBinding,
    string? EventsJson);

public record CreateComponentRequest(
    Guid ScreenId,
    ComponentType Type,
    string Name,
    double PositionX,
    double PositionY,
    string? PropertiesJson = null,
    string? ValidationJson = null,
    string? DataBinding = null,
    string? EventsJson = null);

public record UpdateComponentRequest(
    string Name,
    double PositionX,
    double PositionY,
    string? PropertiesJson = null,
    string? ValidationJson = null,
    string? DataBinding = null,
    string? EventsJson = null);
