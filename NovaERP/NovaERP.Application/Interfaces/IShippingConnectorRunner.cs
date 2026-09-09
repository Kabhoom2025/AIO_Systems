using NovaERP.Application.DTOs;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

/// <summary>Runs one ShippingConnector against one Shipment: builds the outbound request JSON
/// from the connector's Outbound field mappings, calls the connector's configured HTTP
/// endpoint, and parses the response into normalized RateQuoteDtos via the Inbound mappings.
/// Implemented in Infrastructure (JSON-building/HTTP specifics), called once per active
/// connector by ShippingRateService.</summary>
public interface IShippingConnectorRunner
{
    Task<ConnectorRunResultDto> RunAsync(ShippingConnector connector, Shipment shipment, CancellationToken ct = default);

    /// <summary>Sends the given raw JSON body to the connector's configured endpoint/auth as-is —
    /// no field mapping involved — and returns the raw status/body. Powers the Postman-style
    /// "Send" button on the connector edit page.</summary>
    Task<ConnectorTestResultDto> SendRawAsync(ShippingConnector connector, string? rawBody, CancellationToken ct = default);
}
