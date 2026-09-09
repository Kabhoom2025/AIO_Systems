namespace NovaERP.Application.DTOs;

public class CustomerInvoiceLineDto
{
    public int     Id                { get; set; }
    public int     LedgerAccountId   { get; set; }
    public string  LedgerAccountName { get; set; } = string.Empty;
    public string  LedgerAccountCode { get; set; } = string.Empty;
    public string? Description       { get; set; }
    public decimal Amount            { get; set; }
    public int     DisplayOrder      { get; set; }
}

public class CreateCustomerInvoiceLineDto
{
    public int     LedgerAccountId { get; set; }
    public string? Description     { get; set; }
    public decimal Amount          { get; set; }
    public int     DisplayOrder    { get; set; }
}

public class CustomerInvoiceDto
{
    public int      Id                          { get; set; }
    public string   InvoiceNumber               { get; set; } = string.Empty;
    public int      AccountId                   { get; set; }
    public string   AccountName                 { get; set; } = string.Empty;
    public int      ReceivableLedgerAccountId   { get; set; }
    public string   ReceivableLedgerAccountName { get; set; } = string.Empty;
    public DateTime InvoiceDate                 { get; set; }
    public DateTime DueDate                     { get; set; }
    public string   Status                      { get; set; } = string.Empty;
    public int      OwnerId                     { get; set; }
    public string   OwnerName                   { get; set; } = string.Empty;
    public int?     PostedJournalEntryId        { get; set; }
    public int?     PaymentJournalEntryId       { get; set; }
    public List<CustomerInvoiceLineDto> Lines   { get; set; } = new();

    /// <summary>Sum of the lines' Amount — computed on read, not stored.</summary>
    public decimal TotalAmount { get; set; }
}

public class CreateCustomerInvoiceDto
{
    public int      AccountId                 { get; set; }
    public int      ReceivableLedgerAccountId { get; set; }
    public DateTime InvoiceDate               { get; set; } = DateTime.UtcNow.Date;
    public DateTime DueDate                   { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId                   { get; set; }
    public List<CreateCustomerInvoiceLineDto> Lines { get; set; } = new();
}

public class UpdateCustomerInvoiceDto
{
    public int      ReceivableLedgerAccountId { get; set; }
    public DateTime InvoiceDate               { get; set; } = DateTime.UtcNow.Date;
    public DateTime DueDate                   { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId                   { get; set; }
    public List<CreateCustomerInvoiceLineDto> Lines { get; set; } = new();
}

public class ReceiveCustomerInvoicePaymentDto
{
    public int PaymentLedgerAccountId { get; set; }
}
