namespace NovaERP.Domain.Entities;

/// <summary>One product/quantity line within a Shipment. SalesOrderLineId is set only for
/// SalesOrder-sourced shipments, tracing back to the specific order line being fulfilled.</summary>
public class ShipmentLine : BaseEntity
{
    public int     ShipmentId      { get; set; }
    public int     ProductId       { get; set; }
    public decimal Quantity        { get; set; }
    public int?    SalesOrderLineId { get; set; }
    public int     DisplayOrder    { get; set; }

    public Shipment Shipment { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public SalesOrderLine? SalesOrderLine { get; set; }
}
