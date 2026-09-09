using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.Application.Services;

/// <summary>Queries every active ShippingConnector for the org against one Shipment and merges
/// the results — "rate shop" means comparing quotes across carriers/TMSs, not picking just one.
/// A failure in one connector never blocks quotes from the others (see IShippingConnectorRunner,
/// which never throws — it returns a typed Error string instead, same discipline HttpSmsSender
/// already established for external HTTP integrations in this codebase).</summary>
public class ShippingRateService : IShippingRateService
{
    private readonly IShippingConnectorRepository _connectorRepo;
    private readonly IShipmentRepository _shipmentRepo;
    private readonly IShippingConnectorRunner _runner;

    public ShippingRateService(IShippingConnectorRepository connectorRepo, IShipmentRepository shipmentRepo,
        IShippingConnectorRunner runner)
    {
        _connectorRepo = connectorRepo;
        _shipmentRepo = shipmentRepo;
        _runner = runner;
    }

    public async Task<RateQuoteResultDto> GetRatesAsync(int orgId, int shipmentId, CancellationToken ct = default)
    {
        var shipment = await _shipmentRepo.GetByIdAsync(orgId, shipmentId)
            ?? throw new KeyNotFoundException($"Shipment {shipmentId} not found");

        var connectors = await _connectorRepo.GetActiveByOrgAsync(orgId);

        var result = new RateQuoteResultDto();
        if (!connectors.Any())
        {
            result.Errors.Add("No active shipping connectors are configured for this organization.");
            return result;
        }

        foreach (var connector in connectors)
        {
            var runResult = await _runner.RunAsync(connector, shipment, ct);
            if (runResult.Error is not null)
                result.Errors.Add($"{connector.Name}: {runResult.Error}");
            result.Quotes.AddRange(runResult.Quotes);
        }

        return result;
    }
}
