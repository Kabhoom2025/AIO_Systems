namespace NovaERP.Domain.Entities;

/// <summary>Represents one Vendor invited to quote on an RfqRequest. QuotedAmount/Notes/
/// RespondedDate stay null until the vendor's response is recorded via a separate action —
/// creating the invite and recording the quote are deliberately different operations.</summary>
public class RfqVendorQuote : BaseEntity
{
    public int       RfqRequestId  { get; set; }
    public int       VendorId      { get; set; }
    public decimal?  QuotedAmount  { get; set; }
    public string?   Notes         { get; set; }
    public DateTime? RespondedDate { get; set; }

    public RfqRequest RfqRequest { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
}
