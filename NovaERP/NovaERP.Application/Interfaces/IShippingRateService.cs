using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IShippingRateService
{
    Task<RateQuoteResultDto> GetRatesAsync(int orgId, int shipmentId, CancellationToken ct = default);
}
