namespace NovaERP.Application.DTOs;

public class PosSaleLineDto
{
    public int     Id          { get; set; }
    public int     ProductId   { get; set; }
    public string  ProductName { get; set; } = string.Empty;
    public decimal Quantity    { get; set; }
    public decimal UnitPrice   { get; set; }

    /// <summary>Quantity * UnitPrice — computed on read, not stored.</summary>
    public decimal LineTotal   { get; set; }
}

public class CreatePosSaleLineDto
{
    public int     ProductId { get; set; }
    public decimal Quantity  { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PosSaleDto
{
    public int      Id                          { get; set; }
    public string   SaleNumber                  { get; set; } = string.Empty;
    public int      WarehouseId                 { get; set; }
    public string   WarehouseName                { get; set; } = string.Empty;
    public int?     CustomerAccountId            { get; set; }
    public string?  CustomerAccountName          { get; set; }
    public DateTime SaleDate                     { get; set; }
    public int      RevenueLedgerAccountId       { get; set; }
    public string   RevenueLedgerAccountName     { get; set; } = string.Empty;
    public int?     PaymentLedgerAccountId       { get; set; }
    public string?  PaymentLedgerAccountName     { get; set; }
    public string   Status                       { get; set; } = string.Empty;
    public int      OwnerId                      { get; set; }
    public string   OwnerName                    { get; set; } = string.Empty;
    public int?     PostedJournalEntryId         { get; set; }
    public List<PosSaleLineDto> Lines            { get; set; } = new();

    /// <summary>Sum of the lines' LineTotal — computed on read, not stored.</summary>
    public decimal TotalAmount { get; set; }
}

public class CreatePosSaleDto
{
    public int      WarehouseId            { get; set; }
    public int?     CustomerAccountId      { get; set; }
    public DateTime SaleDate               { get; set; } = DateTime.UtcNow.Date;
    public int      RevenueLedgerAccountId { get; set; }
    public int      OwnerId                { get; set; }
    public List<CreatePosSaleLineDto> Lines { get; set; } = new();
}

public class UpdatePosSaleDto
{
    public int      WarehouseId            { get; set; }
    public int?     CustomerAccountId      { get; set; }
    public DateTime SaleDate               { get; set; }
    public int      RevenueLedgerAccountId { get; set; }
    public int      OwnerId                { get; set; }
    public List<CreatePosSaleLineDto> Lines { get; set; } = new();
}

public class CompletePosSaleDto
{
    public int PaymentLedgerAccountId { get; set; }
}
