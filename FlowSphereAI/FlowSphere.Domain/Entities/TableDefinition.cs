using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

/// <summary>A drag-and-drop-built data table (columns) inside a Workspace. Not ITenantScoped
/// itself - scoped transitively through WorkspaceId, same convention as AppDefinition.</summary>
public class TableDefinition : BaseEntity
{
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>{ "columns": [ { "key", "type", "label", "required", "unique", "defaultValue", "options" }, ... ] } -
    /// the drag-and-drop column schema, source of truth for the table builder canvas.</summary>
    public string SchemaJson { get; set; } = "{\"columns\":[]}";

    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }

    public int CreatedByUserId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public static TableDefinition Create(int workspaceId, string name, string? description, int createdByUserId)
    {
        return new TableDefinition
        {
            WorkspaceId = workspaceId,
            Name = name,
            Description = description,
            CreatedByUserId = createdByUserId,
        };
    }

    public void UpdateSchema(string schemaJson)
    {
        SchemaJson = schemaJson;
    }

    public void Publish()
    {
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
    }
}
