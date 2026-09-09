using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

public class WorkflowDefinition : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int CreatedByUserId { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<WorkflowVersion> Versions { get; set; } = new List<WorkflowVersion>();

    public static WorkflowDefinition Create(int organizationId, string name, string? description, int createdByUserId)
    {
        return new WorkflowDefinition
        {
            OrganizationId = organizationId,
            Name = name,
            Description = description,
            CreatedByUserId = createdByUserId,
            IsEnabled = true
        };
    }

    /// <summary>Creates the next monotonically-numbered Draft version. Caller must have
    /// Versions loaded (Include) so VersionNumber is computed correctly.</summary>
    public WorkflowVersion CreateNextVersion(string graphJson)
    {
        var nextVersionNumber = Versions.Count == 0 ? 1 : Versions.Max(v => v.VersionNumber) + 1;

        var version = new WorkflowVersion
        {
            WorkflowDefinitionId = Id,
            VersionNumber = nextVersionNumber,
            Status = VersionStatus.Draft,
            GraphJson = graphJson
        };

        Versions.Add(version);
        return version;
    }

    /// <summary>Enforces the "0 or 1 Published version" invariant: demotes whichever version is
    /// currently Published (if any) to Archived before promoting the requested one. Caller must
    /// have Versions loaded.</summary>
    public void Publish(int versionId)
    {
        var target = Versions.FirstOrDefault(v => v.Id == versionId);
        if (target is null)
        {
            throw new DomainException($"Version {versionId} does not belong to workflow {Id}.");
        }

        if (target.Status == VersionStatus.Published)
        {
            return;
        }

        var currentlyPublished = Versions.FirstOrDefault(v => v.Status == VersionStatus.Published);
        currentlyPublished?.Archive();

        target.MarkPublished();
    }
}
