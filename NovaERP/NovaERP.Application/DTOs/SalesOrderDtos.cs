namespace NovaERP.Application.DTOs;

public class SalesOrderLineDto
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

public class CreateSalesOrderLineDto
{
    public string  ItemName     { get; set; } = string.Empty;
    public int?    ProductId    { get; set; }
    public decimal Quantity     { get; set; }
    public decimal UnitPrice    { get; set; }
    public int?    TaxCodeId    { get; set; }
    public int     DisplayOrder { get; set; }
}

public class SalesOrderDto
{
    public int       Id              { get; set; }
    public string    OrderNumber     { get; set; } = string.Empty;
    public int       AccountId       { get; set; }
    public string    AccountName     { get; set; } = string.Empty;
    public int?      OpportunityId   { get; set; }
    public string?   OpportunityName { get; set; }
    public string    Status          { get; set; } = string.Empty;
    public DateTime  OrderDate       { get; set; }
    public int       OwnerId         { get; set; }
    public string    OwnerName       { get; set; } = string.Empty;
    public List<SalesOrderLineDto> Lines { get; set; } = new();

    public decimal Subtotal  { get; set; }
    public decimal TaxTotal  { get; set; }
    public decimal GrandTotal { get; set; }
}

public class CreateSalesOrderDto
{
    public int       AccountId     { get; set; }
    public int?      OpportunityId { get; set; }
    public DateTime  OrderDate     { get; set; } = DateTime.UtcNow.Date;
    public int       OwnerId       { get; set; }
    public List<CreateSalesOrderLineDto> Lines { get; set; } = new();
}

/// <summary>AccountId is immutable after creation — same "identifying field frozen on edit"
/// convention as ExchangeRate's currency pair / TaxCode's Code / Opportunity's AccountId.
/// Lines are replaced wholesale, same as TaxCode's Components. Only permitted while the
/// order's Status is "Draft" — enforced in the service, not here.</summary>
public class UpdateSalesOrderDto
{
    public int?      OpportunityId { get; set; }
    public DateTime  OrderDate     { get; set; } = DateTime.UtcNow.Date;
    public int       OwnerId       { get; set; }
    public List<CreateSalesOrderLineDto> Lines { get; set; } = new();
}

/// <summary>A quick "how is this salesperson doing" snapshot for the User Detail page —
/// aggregated from every SalesOrder they own, not a stored/cached value.</summary>
public class UserSalesSummaryDto
{
    public int     TotalOrders      { get; set; }
    public int     DraftOrders      { get; set; }
    public int     ConfirmedOrders  { get; set; }
    public int     CancelledOrders  { get; set; }
    public decimal TotalSalesValue  { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<UserRecentOrderDto> RecentOrders { get; set; } = new();
}

public class UserRecentOrderDto
{
    public int      Id          { get; set; }
    public string   OrderNumber { get; set; } = string.Empty;
    public string   AccountName { get; set; } = string.Empty;
    public string   Status      { get; set; } = string.Empty;
    public DateTime OrderDate   { get; set; }
    public decimal  GrandTotal  { get; set; }
}
