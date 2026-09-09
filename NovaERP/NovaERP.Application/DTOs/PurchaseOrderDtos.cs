namespace NovaERP.Application.DTOs;

public class PurchaseOrderLineDto
{
    public int      Id             { get; set; }
    public string   ItemName       { get; set; } = string.Empty;
    public int?     ProductId      { get; set; }
    public string?  ProductName    { get; set; }
    public decimal  Quantity       { get; set; }
    public decimal  UnitPrice      { get; set; }
    public int?     TaxCodeId      { get; set; }
    public string?  TaxCodeName    { get; set; }
    public decimal  TaxRatePercent { get; set; }
    public int      DisplayOrder   { get; set; }

    /// <summary>Quantity * UnitPrice — computed on read, not stored.</summary>
    public decimal LineSubtotal { get; set; }

    /// <summary>LineSubtotal * TaxRatePercent / 100 — computed on read, not stored.</summary>
    public decimal LineTax { get; set; }

    public decimal LineTotal { get; set; }
}

public class CreatePurchaseOrderLineDto
{
    public string  ItemName     { get; set; } = string.Empty;
    public int?    ProductId    { get; set; }
    public decimal Quantity     { get; set; }
    public decimal UnitPrice    { get; set; }
    public int?    TaxCodeId    { get; set; }
    public int     DisplayOrder { get; set; }
}

public class PurchaseOrderDto
{
    public int       Id            { get; set; }
    public string    PoNumber      { get; set; } = string.Empty;
    public int       VendorId      { get; set; }
    public string    VendorName    { get; set; } = string.Empty;
    public int?      RfqRequestId  { get; set; }
    public string?   RfqNumber     { get; set; }
    public string    Status        { get; set; } = string.Empty;
    public DateTime  OrderDate     { get; set; }
    public int       OwnerId       { get; set; }
    public string    OwnerName     { get; set; } = string.Empty;
    public List<PurchaseOrderLineDto> Lines { get; set; } = new();

    public decimal Subtotal   { get; set; }
    public decimal TaxTotal   { get; set; }
    public decimal GrandTotal { get; set; }
}

public class CreatePurchaseOrderDto
{
    public int      VendorId     { get; set; }
    public int?     RfqRequestId { get; set; }
    public DateTime OrderDate    { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId      { get; set; }
    public List<CreatePurchaseOrderLineDto> Lines { get; set; } = new();
}

/// <summary>VendorId is immutable after creation — same "identifying field frozen on edit"
/// convention as SalesOrder's AccountId. Lines are replaced wholesale, same as SalesOrder's
/// Lines. Only permitted while the order's Status is "Draft" — enforced in the service.</summary>
public class UpdatePurchaseOrderDto
{
    public int?     RfqRequestId { get; set; }
    public DateTime OrderDate    { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId      { get; set; }
    public List<CreatePurchaseOrderLineDto> Lines { get; set; } = new();
}
