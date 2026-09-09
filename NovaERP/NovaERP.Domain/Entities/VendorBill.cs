namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped bill from a Vendor. Approving it posts a real JournalEntry (Debit
/// each line's account, Credit PayableLedgerAccountId); paying it posts a second entry (Debit
/// payable, Credit whichever cash/bank account is chosen at payment time). Lines are owned/
/// replaced wholesale, same as JournalEntry's Lines.</summary>
public class VendorBill : BaseEntity
{
    public int      OrganizationId         { get; set; }
    public string   BillNumber             { get; set; } = string.Empty;
    public int      VendorId               { get; set; }
    public int      PayableLedgerAccountId { get; set; }
    public DateTime BillDate               { get; set; } = DateTime.UtcNow.Date;
    public DateTime DueDate                { get; set; } = DateTime.UtcNow.Date;
    public string   Status                 { get; set; } = "Draft"; // Draft | Approved | Paid | Voided
    public int      OwnerId                { get; set; }

    /// <summary>Set by ApproveAsync/PayAsync respectively — traceability back to the GL
    /// entries this bill produced. Both null until those actions run.</summary>
    public int? PostedJournalEntryId  { get; set; }
    public int? PaymentJournalEntryId { get; set; }

    public Organization Organization { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public LedgerAccount PayableLedgerAccount { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public JournalEntry? PostedJournalEntry { get; set; }
    public JournalEntry? PaymentJournalEntry { get; set; }
    public ICollection<VendorBillLine> Lines { get; set; } = new List<VendorBillLine>();
}
