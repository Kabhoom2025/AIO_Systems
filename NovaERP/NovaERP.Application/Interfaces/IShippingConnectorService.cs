using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IShippingConnectorService
{
    Task<List<ShippingConnectorDto>> GetAllAsync(int orgId);
    Task<ShippingConnectorDto> GetByIdAsync(int orgId, int id);
    Task<ShippingConnectorDto> CreateAsync(int orgId, SaveShippingConnectorDto dto);
    Task<ShippingConnectorDto> UpdateAsync(int orgId, int id, SaveShippingConnectorDto dto);
    Task DeleteAsync(int orgId, int id);

    /// <summary>Postman-style "Send" — tries the draft connector form (optionally against an
    /// existing saved connector's id, to inherit any secret fields left blank in the form)
    /// without requiring Save first.</summary>
    Task<ConnectorTestResultDto> TestAsync(int orgId, int? connectorId, TestShippingConnectorDto dto);
}
