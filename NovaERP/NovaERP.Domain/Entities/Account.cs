namespace NovaERP.Domain.Entities;

/// <summary>A customer/prospect organization. Anchors Contacts and Opportunities.</summary>
public class Account : BaseEntity
{
    public int     OrganizationId { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string? Industry       { get; set; }
    public string? Website        { get; set; }
    public string? Phone          { get; set; }
    public int     OwnerId        { get; set; }

    public Organization Organization { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}
