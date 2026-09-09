namespace Pharmacy.Domain.Entities;

public class Notification : BaseEntity
{
    public int      OrganizationId     { get; set; }
    public string   Type               { get; set; } = string.Empty; // LowStock | ExpiringBatch
    public string   Title              { get; set; } = string.Empty;
    public string   Message            { get; set; } = string.Empty;
    public string?  RelatedEntityType  { get; set; } // Medicine | MedicineBatch
    public int?     RelatedEntityId    { get; set; }
    public bool     IsRead             { get; set; }

    public Organization Organization { get; set; } = null!;
}
