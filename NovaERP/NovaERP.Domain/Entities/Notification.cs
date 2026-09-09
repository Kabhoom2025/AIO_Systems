namespace NovaERP.Domain.Entities;

public class Notification : BaseEntity
{
    public int     OrganizationId { get; set; }
    public int?    UserId         { get; set; } // null = broadcast
    public string  Title          { get; set; } = string.Empty;
    public string  Message        { get; set; } = string.Empty;
    public string  Type           { get; set; } = "Info";
    public string? Link           { get; set; }
    public bool    IsRead         { get; set; }

    public Organization Organization { get; set; } = null!;
    public User?        User         { get; set; }
}
