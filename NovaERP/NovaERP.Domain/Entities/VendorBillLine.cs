namespace NovaERP.Domain.Entities;

/// <summary>One expense/asset-account line within a VendorBill — the account debited when
/// the bill is approved.</summary>
public class VendorBillLine : BaseEntity
{
    public int     VendorBillId    { get; set; }
    public int     LedgerAccountId { get; set; }
    public string? Description     { get; set; }
    public decimal Amount          { get; set; }
    public int     DisplayOrder    { get; set; }

    public VendorBill VendorBill { get; set; } = null!;
    public LedgerAccount LedgerAccount { get; set; } = null!;
}
