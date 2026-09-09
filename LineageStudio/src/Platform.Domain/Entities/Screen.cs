namespace Platform.Domain.Entities;

public class Screen
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public required string Name { get; set; }
    public required string Route { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public AppDefinition? Application { get; set; }
    public ICollection<UiComponent> Components { get; set; } = new List<UiComponent>();
}
