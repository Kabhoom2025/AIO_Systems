namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped Request for Quotation. Items describe what's being requested (no
/// pricing); Quotes track each invited Vendor's response. Stops at "Closed" — turning the
/// winning vendor's quote into a Purchase Order belongs to the Purchase module (not built yet).</summary>
public class RfqRequest : BaseEntity
{
    public int       OrganizationId    { get; set; }
    public string    RfqNumber         { get; set; } = string.Empty;
    public string    Title             { get; set; } = string.Empty;
    public string?   Description       { get; set; }
    public string    Status            { get; set; } = "Draft"; // Draft | Sent | Closed | Cancelled
    public DateTime  IssueDate         { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ResponseDeadline  { get; set; }
    public int       OwnerId           { get; set; }
    public int?      WinningVendorId   { get; set; }

    public Organization Organization { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public Vendor? WinningVendor { get; set; }
    public ICollection<RfqItem> Items { get; set; } = new List<RfqItem>();
    public ICollection<RfqVendorQuote> Quotes { get; set; } = new List<RfqVendorQuote>();
}
