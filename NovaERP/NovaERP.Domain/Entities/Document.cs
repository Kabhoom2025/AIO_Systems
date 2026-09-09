namespace NovaERP.Domain.Entities;

/// <summary>Generic org-scoped attachment. EntityType/EntityId form an optional polymorphic
/// association (e.g. EntityType = "Branch", EntityId = 3) so any future module can attach
/// documents without a dedicated join table.</summary>
public class Document : BaseEntity
{
    public int      OrganizationId   { get; set; }
    public string   FileName         { get; set; } = string.Empty;
    public string   ContentType      { get; set; } = string.Empty;
    public long     SizeBytes        { get; set; }
    public string   StoragePath      { get; set; } = string.Empty;
    public string?  EntityType       { get; set; }
    public int?     EntityId         { get; set; }
    public int      UploadedByUserId { get; set; }
    public DateTime UploadedDate     { get; set; } = DateTime.UtcNow;

    public Organization Organization { get; set; } = null!;
}
