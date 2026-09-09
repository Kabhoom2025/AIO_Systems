namespace AIO_Systems.Domain.Entities;

/// <summary>
/// Provides common audit fields for entities that are created and tracked over time.
/// Not all domain entities inherit this — only those with lifecycle timestamps.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
