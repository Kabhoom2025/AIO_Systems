using Platform.Domain.Enums;

namespace Platform.Domain.Entities;

public class UiComponent
{
    public Guid Id { get; set; }
    public Guid ScreenId { get; set; }
    public ComponentType Type { get; set; }
    public required string Name { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }

    /// <summary>Free-form component config (label, placeholder, options, etc.) as JSON.</summary>
    public string? PropertiesJson { get; set; }

    /// <summary>Validation rules (required, min/max, regex) as JSON.</summary>
    public string? ValidationJson { get; set; }

    /// <summary>Field name this component's value binds to, used by the mapping designer.</summary>
    public string? DataBinding { get; set; }

    /// <summary>Event handlers (onClick -> apiId, etc.) as JSON.</summary>
    public string? EventsJson { get; set; }

    public Screen? Screen { get; set; }
}
