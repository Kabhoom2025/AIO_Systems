using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

public class Workspace : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreatedByUserId { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<AppDefinition> Apps { get; set; } = new List<AppDefinition>();
    public ICollection<TableDefinition> Tables { get; set; } = new List<TableDefinition>();

    public static Workspace Create(int organizationId, string name, string? description, int createdByUserId)
    {
        return new Workspace
        {
            OrganizationId = organizationId,
            Name = name,
            Description = description,
            CreatedByUserId = createdByUserId,
        };
    }

    public void Rename(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
