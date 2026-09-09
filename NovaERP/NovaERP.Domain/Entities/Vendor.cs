namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped supplier. Same shape as CRM's Account, but for the buy-side. UserId
/// is optional — not every vendor has a Vendor Portal login; mirrors Contact.UserId.</summary>
public class Vendor : BaseEntity
{
    public int     OrganizationId { get; set; }
    public int?    UserId         { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string? Category       { get; set; }
    public string? ContactEmail   { get; set; }
    public string? ContactPhone   { get; set; }
    public string? Address        { get; set; }
    public bool    IsActive       { get; set; } = true;
    public int     OwnerId        { get; set; }

    public Organization Organization { get; set; } = null!;
    public User?    User  { get; set; }
    public User Owner { get; set; } = null!;
}
