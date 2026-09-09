namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped physical shipment out of a Warehouse — either fulfilling a
/// Confirmed SalesOrder (to a customer's ship-to address) or a Transfer Order (to another
/// Warehouse). Shipping it is what actually moves stock (Issue at WarehouseId, plus a Receipt
/// at DestinationWarehouseId for TransferOrder shipments) — this is the fulfillment stage
/// SalesOrder.Confirm used to short-circuit before this module existed.</summary>
public class Shipment : BaseEntity
{
    public int      OrganizationId         { get; set; }
    public string   ShipmentNumber         { get; set; } = string.Empty;
    public int      WarehouseId            { get; set; }
    public string   SourceType             { get; set; } = string.Empty; // SalesOrder | TransferOrder
    public int?     SalesOrderId           { get; set; }
    public int?     DestinationWarehouseId { get; set; }

    public string? ShipToName         { get; set; }
    public string? ShipToContactName  { get; set; }
    public string? ShipToEmail        { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity         { get; set; }
    public string? ShipToState        { get; set; }
    public string? ShipToPostalCode   { get; set; }
    public string? ShipToCountry      { get; set; }
    public string? ShipToPhone        { get; set; }
    public string? ShipToTaxType      { get; set; }
    public string? ShipToTaxCountry   { get; set; }
    public string? ShipToTaxId        { get; set; }

    /// <summary>Optional per-shipment override of the origin address — when any field here is
    /// set, it takes precedence over the parent Warehouse's own address for that field (see
    /// ShippingConnectorHttpRunner.GetScalarValue). Null/blank means "use the Warehouse's
    /// address as before" — most shipments never need this, it exists for the rare case where
    /// a shipment actually ships from somewhere other than its Warehouse's registered address.</summary>
    public string? ShipFromName         { get; set; }
    public string? ShipFromContactName  { get; set; }
    public string? ShipFromEmail        { get; set; }
    public string? ShipFromAddressLine1 { get; set; }
    public string? ShipFromAddressLine2 { get; set; }
    public string? ShipFromCity         { get; set; }
    public string? ShipFromState        { get; set; }
    public string? ShipFromPostalCode   { get; set; }
    public string? ShipFromCountry      { get; set; }
    public string? ShipFromPhone        { get; set; }

    public DateTime ShipDate        { get; set; } = DateTime.UtcNow.Date;
    public string?  Carrier         { get; set; }
    public string?  TrackingNumber  { get; set; }
    public bool     IsBlindShipment { get; set; }
    public string   Status          { get; set; } = "Open"; // Open | Picked | Shipped | Delivered | Cancelled
    public int      OwnerId         { get; set; }
    /// <summary>Set once this shipment is assigned to a vehicle's DeliveryLoad — optional,
    /// a Shipment can still be picked/shipped the old way without ever being loaded onto a
    /// tracked vehicle.</summary>
    public int?     DeliveryLoadId  { get; set; }

    public Organization Organization { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public SalesOrder? SalesOrder { get; set; }
    public Warehouse? DestinationWarehouse { get; set; }
    public User Owner { get; set; } = null!;
    public DeliveryLoad? DeliveryLoad { get; set; }
    public ICollection<ShipmentLine> Lines { get; set; } = new List<ShipmentLine>();
    public ICollection<ShipmentPackage> Packages { get; set; } = new List<ShipmentPackage>();
}
