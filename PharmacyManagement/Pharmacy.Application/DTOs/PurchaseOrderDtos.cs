namespace Pharmacy.Application.DTOs;

public class PurchaseOrderDto
{
    public int      Id                   { get; set; }
    public int      SupplierId           { get; set; }
    public string   SupplierName         { get; set; } = string.Empty;
    public string   PoNumber             { get; set; } = string.Empty;
    public DateTime OrderDate            { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string   Status               { get; set; } = string.Empty;
    public decimal  TotalAmount          { get; set; }
    public string?  Notes                { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class PurchaseOrderItemDto
{
    public int     Id               { get; set; }
    public int     MedicineId       { get; set; }
    public string  MedicineName     { get; set; } = string.Empty;
    public int     Quantity         { get; set; }
    public decimal UnitPrice        { get; set; }
    public int     ReceivedQuantity { get; set; }
}

public class CreatePurchaseOrderDto
{
    public int      SupplierId           { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string?  Notes                { get; set; }
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemDto
{
    public int     MedicineId { get; set; }
    public int     Quantity   { get; set; }
    public decimal UnitPrice  { get; set; }
}

public class UpdatePurchaseOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}
