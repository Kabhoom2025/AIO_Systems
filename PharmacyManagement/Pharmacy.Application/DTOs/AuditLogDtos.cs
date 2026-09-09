namespace Pharmacy.Application.DTOs;

public class AuditLogDto
{
    public int      Id         { get; set; }
    public DateTime Timestamp  { get; set; }
    public string   UserName   { get; set; } = string.Empty;
    public string   Action     { get; set; } = string.Empty;
    public string   EntityType { get; set; } = string.Empty;
    public int?     EntityId   { get; set; }
}
