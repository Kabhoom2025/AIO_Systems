namespace NovaERP.Domain.Entities;

/// <summary>One field-mapping row on a ShippingConnector. Outbound rows map a known NovaERP
/// field (e.g. "Shipment.ShipToPostalCode", "Package.WeightKg") to a path in the external
/// request JSON; Inbound rows map a path in the external response JSON back to a known
/// normalized quote field (e.g. "Quote.Price"). ExternalPath supports at most one "[]" array
/// wildcard (e.g. "packages[].weight", "rates[].price") — see JsonPathWalker. Outbound rows can
/// also use NovaField "Constant" — a literal value (in ConstantValue) rather than data pulled
/// from the Shipment/Package/Warehouse, for TMS-specific fixed fields (e.g. a location/account
/// code) that aren't shipment data at all.</summary>
public class ShippingConnectorFieldMapping : BaseEntity
{
    public int     ConnectorId    { get; set; }
    public string  Direction      { get; set; } = string.Empty; // Outbound | Inbound
    public string  NovaField      { get; set; } = string.Empty;
    public string  ExternalPath   { get; set; } = string.Empty;
    public string  Transform      { get; set; } = "None"; // None | KgToLb | LbToKg | CmToIn | InToCm | Uppercase | Lowercase
    public string? ConstantValue  { get; set; } // only used when NovaField == "Constant"

    public ShippingConnector Connector { get; set; } = null!;
}
