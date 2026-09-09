namespace NovaERP.Application.DTOs;

public class VendorBillLineDto
{
    public int     Id                { get; set; }
    public int     LedgerAccountId   { get; set; }
    public string  LedgerAccountName { get; set; } = string.Empty;
    public string  LedgerAccountCode { get; set; } = string.Empty;
    public string? Description       { get; set; }
    public decimal Amount            { get; set; }
    public int     DisplayOrder      { get; set; }
}

public class CreateVendorBillLineDto
{
    public int     LedgerAccountId { get; set; }
    public string? Description     { get; set; }
    public decimal Amount          { get; set; }
    public int     DisplayOrder    { get; set; }
}

public class VendorBillDto
{
    public int      Id                       { get; set; }
    public string   BillNumber               { get; set; } = string.Empty;
    public int      VendorId                 { get; set; }
    public string   VendorName               { get; set; } = string.Empty;
    public int      PayableLedgerAccountId   { get; set; }
    public string   PayableLedgerAccountName { get; set; } = string.Empty;
    public DateTime BillDate                 { get; set; }
    public DateTime DueDate                  { get; set; }
    public string   Status                   { get; set; } = string.Empty;
    public int      OwnerId                  { get; set; }
    public string   OwnerName                { get; set; } = string.Empty;
    public int?     PostedJournalEntryId     { get; set; }
    public int?     PaymentJournalEntryId    { get; set; }
    public List<VendorBillLineDto> Lines     { get; set; } = new();

    /// <summary>Sum of the lines' Amount — computed on read, not stored.</summary>
    public decimal TotalAmount { get; set; }
}

public class CreateVendorBillDto
{
    public int      VendorId               { get; set; }
    public int      PayableLedgerAccountId { get; set; }
    public DateTime BillDate               { get; set; } = DateTime.UtcNow.Date;
    public DateTime DueDate                { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId                { get; set; }
    public List<CreateVendorBillLineDto> Lines { get; set; } = new();
}

public class UpdateVendorBillDto
{
    public int      PayableLedgerAccountId { get; set; }
    public DateTime BillDate               { get; set; } = DateTime.UtcNow.Date;
    public DateTime DueDate                { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId                { get; set; }
    public List<CreateVendorBillLineDto> Lines { get; set; } = new();
}

public class PayVendorBillDto
{
    public int PaymentLedgerAccountId { get; set; }
}
