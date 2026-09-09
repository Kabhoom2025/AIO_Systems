namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped point-of-sale register transaction. Completing it decrements stock
/// (one Issue StockMovement per line, same bypass-repository pattern as Shipment) and posts a
/// real JournalEntry (Debit PaymentLedgerAccountId, Credit RevenueLedgerAccountId); refunding
/// reverses both. Unlike Project.Status, Complete/Refund/Cancel are explicit actions since a
/// completed sale is a one-way commercial event, the same "locked once past Draft" shape as
/// VendorBill/CustomerInvoice.</summary>
public class PosSale : BaseEntity
{
    public int      OrganizationId         { get; set; }
    public string   SaleNumber             { get; set; } = string.Empty;
    public int      WarehouseId            { get; set; }
    public int?     CustomerAccountId      { get; set; }
    public DateTime SaleDate               { get; set; } = DateTime.UtcNow.Date;
    public int      RevenueLedgerAccountId { get; set; }
    public int?     PaymentLedgerAccountId { get; set; }
    public string   Status                 { get; set; } = "Draft"; // Draft | Completed | Refunded | Cancelled
    public int      OwnerId                { get; set; }

    /// <summary>Set by CompleteAsync — traceability back to the GL entry the sale produced.</summary>
    public int? PostedJournalEntryId { get; set; }

    public Organization   Organization           { get; set; } = null!;
    public Warehouse      Warehouse              { get; set; } = null!;
    public Account?       CustomerAccount        { get; set; }
    public LedgerAccount  RevenueLedgerAccount    { get; set; } = null!;
    public LedgerAccount? PaymentLedgerAccount    { get; set; }
    public User           Owner                  { get; set; } = null!;
    public JournalEntry?  PostedJournalEntry      { get; set; }
    public ICollection<PosSaleLine> Lines { get; set; } = new List<PosSaleLine>();
}
