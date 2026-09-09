namespace HRMS.Domain.Entities;

public class Notification : BaseEntity
{
    public int     OrganizationId { get; set; }
    public int?    UserId         { get; set; } // null = broadcast to whole org
    public string  Title          { get; set; } = string.Empty;
    public string  Message        { get; set; } = string.Empty;
    public string  Type           { get; set; } = "Info"; // Info | Success | Warning | Error
    public string? Link           { get; set; }           // in-app route, e.g. /leaves
    public bool    IsRead         { get; set; }

    public Organization Organization { get; set; } = null!;
    public User?        User         { get; set; }
}
