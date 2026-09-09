namespace Pharmacy.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int      UserId         { get; set; }
    public string   UserName       { get; set; } = string.Empty;
    public string   HttpMethod     { get; set; } = string.Empty;
    public string   Action         { get; set; } = string.Empty; // Create | Update | Delete | Other
    public string   EntityType     { get; set; } = string.Empty;
    public int?     EntityId       { get; set; }
    public DateTime Timestamp      { get; set; } = DateTime.UtcNow;
}
