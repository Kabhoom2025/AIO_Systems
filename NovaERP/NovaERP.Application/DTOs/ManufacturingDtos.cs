namespace NovaERP.Application.DTOs;

public class BomComponentDto
{
    public int     Id                 { get; set; }
    public int     ComponentProductId { get; set; }
    public string  ComponentProductName { get; set; } = string.Empty;
    public string  ComponentProductSku  { get; set; } = string.Empty;
    public decimal Quantity           { get; set; }
    public int     DisplayOrder       { get; set; }
}

public class CreateBomComponentDto
{
    public int     ComponentProductId { get; set; }
    public decimal Quantity           { get; set; }
    public int     DisplayOrder       { get; set; }
}

public class BillOfMaterialDto
{
    public int    Id          { get; set; }
    public int    ProductId   { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku  { get; set; } = string.Empty;
    public bool   IsActive    { get; set; }
    public List<BomComponentDto> Components { get; set; } = new();
}

public class CreateBillOfMaterialDto
{
    public int  ProductId { get; set; }
    public bool IsActive  { get; set; } = true;
    public List<CreateBomComponentDto> Components { get; set; } = new();
}

/// <summary>Components are owned by the BOM and replaced wholesale on update — same
/// "replace-all" approach as UpdateTaxCodeDto's Components.</summary>
public class UpdateBillOfMaterialDto
{
    public bool IsActive { get; set; } = true;
    public List<CreateBomComponentDto> Components { get; set; } = new();
}

public class ProductionOrderDto
{
    public int      Id            { get; set; }
    public string   MoNumber      { get; set; } = string.Empty;
    public int      ProductId     { get; set; }
    public string   ProductName   { get; set; } = string.Empty;
    public int      WarehouseId   { get; set; }
    public string   WarehouseName { get; set; } = string.Empty;
    public decimal  Quantity      { get; set; }
    public string   Status        { get; set; } = string.Empty;
    public DateTime OrderDate     { get; set; }
    public int      OwnerId       { get; set; }
    public string   OwnerName     { get; set; } = string.Empty;
}

public class CreateProductionOrderDto
{
    public int      ProductId   { get; set; }
    public int      WarehouseId { get; set; }
    public decimal  Quantity    { get; set; }
    public DateTime OrderDate   { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId     { get; set; }
}

public class UpdateProductionOrderDto
{
    public int      WarehouseId { get; set; }
    public decimal  Quantity    { get; set; }
    public DateTime OrderDate   { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId     { get; set; }
}
