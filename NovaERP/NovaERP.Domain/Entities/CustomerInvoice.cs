namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped invoice to a CRM Account (customer). Sending it posts a real
/// JournalEntry (Debit ReceivableLedgerAccountId, Credit each line's account); receiving
/// payment posts a second entry (Debit whichever cash/bank account is chosen, Credit
/// receivable). Direct mirror of VendorBill, with Debit/Credit reversed and Vendor swapped
/// for the CRM Account.</summary>
public class CustomerInvoice : BaseEntity
{
    public int      OrganizationId           { get; set; }
    public string   InvoiceNumber            { get; set; } = string.Empty;
    public int      AccountId                { get; set; }
    public int      ReceivableLedgerAccountId { get; set; }
    public DateTime InvoiceDate              { get; set; } = DateTime.UtcNow.Date;
    public DateTime DueDate                  { get; set; } = DateTime.UtcNow.Date;
    public string   Status                   { get; set; } = "Draft"; // Draft | Sent | Paid | Voided
    public int      OwnerId                  { get; set; }

    /// <summary>Set by SendAsync/ReceivePaymentAsync respectively — traceability back to the
    /// GL entries this invoice produced. Both null until those actions run.</summary>
    public int? PostedJournalEntryId  { get; set; }
    public int? PaymentJournalEntryId { get; set; }

    public Organization Organization { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public LedgerAccount ReceivableLedgerAccount { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public JournalEntry? PostedJournalEntry { get; set; }
    public JournalEntry? PaymentJournalEntry { get; set; }
    public ICollection<CustomerInvoiceLine> Lines { get; set; } = new List<CustomerInvoiceLine>();
}
