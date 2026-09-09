namespace NovaERP.Domain.Entities;

/// <summary>One revenue/other-account line within a CustomerInvoice — the account credited
/// when the invoice is sent.</summary>
public class CustomerInvoiceLine : BaseEntity
{
    public int     CustomerInvoiceId { get; set; }
    public int     LedgerAccountId   { get; set; }
    public string? Description       { get; set; }
    public decimal Amount            { get; set; }
    public int     DisplayOrder      { get; set; }

    public CustomerInvoice CustomerInvoice { get; set; } = null!;
    public LedgerAccount LedgerAccount { get; set; } = null!;
}
