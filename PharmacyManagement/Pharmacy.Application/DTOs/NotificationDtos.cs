namespace Pharmacy.Application.DTOs;

public class NotificationDto
{
    public int      Id                { get; set; }
    public string   Type              { get; set; } = string.Empty;
    public string   Title             { get; set; } = string.Empty;
    public string   Message           { get; set; } = string.Empty;
    public string?  RelatedEntityType { get; set; }
    public int?     RelatedEntityId   { get; set; }
    public bool     IsRead            { get; set; }
    public DateTime CreatedDate       { get; set; }
}
