using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class ShippingConnectorService : IShippingConnectorService
{
    private readonly IShippingConnectorRepository _repo;
    private readonly IShippingConnectorRunner _runner;
    private readonly IValidator<SaveShippingConnectorDto> _validator;

    public ShippingConnectorService(IShippingConnectorRepository repo, IShippingConnectorRunner runner, IValidator<SaveShippingConnectorDto> validator)
    {
        _repo = repo;
        _runner = runner;
        _validator = validator;
    }

    public async Task<List<ShippingConnectorDto>> GetAllAsync(int orgId)
    {
        var connectors = await _repo.GetAllByOrgAsync(orgId);
        return connectors.Select(ToDto).ToList();
    }

    public async Task<ShippingConnectorDto> GetByIdAsync(int orgId, int id)
    {
        var connector = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipping connector {id} not found");
        return ToDto(connector);
    }

    public async Task<ShippingConnectorDto> CreateAsync(int orgId, SaveShippingConnectorDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        var connector = new ShippingConnector { OrganizationId = orgId };
        ApplyBasicFields(connector, dto);
        ReplaceMappings(connector, dto.FieldMappings);

        _repo.Add(connector);
        await _repo.SaveChangesAsync();
        return ToDto(connector);
    }

    public async Task<ShippingConnectorDto> UpdateAsync(int orgId, int id, SaveShippingConnectorDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        var connector = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipping connector {id} not found");

        ApplyBasicFields(connector, dto);
        ReplaceMappings(connector, dto.FieldMappings);
        connector.UpdatedDate = DateTime.UtcNow;

        _repo.Update(connector);
        await _repo.SaveChangesAsync();
        return ToDto(connector);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var connector = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipping connector {id} not found");
        _repo.Remove(connector);
        await _repo.SaveChangesAsync();
    }

    public async Task<ConnectorTestResultDto> TestAsync(int orgId, int? connectorId, TestShippingConnectorDto dto)
    {
        ShippingConnector connector;
        if (connectorId.HasValue)
        {
            // Editing a saved connector: start from the stored row so secret fields left blank
            // in the draft form (the "keep unchanged" convention) still carry their real value
            // into the test call instead of being sent as empty.
            connector = await _repo.GetByIdAsync(orgId, connectorId.Value)
                ?? throw new KeyNotFoundException($"Shipping connector {connectorId} not found");
        }
        else
        {
            connector = new ShippingConnector { OrganizationId = orgId };
        }

        ApplyBasicFields(connector, dto.Connector);
        return await _runner.SendRawAsync(connector, dto.RequestBody, CancellationToken.None);
    }

    private static void ApplyBasicFields(ShippingConnector connector, SaveShippingConnectorDto dto)
    {
        connector.Name = dto.Name;
        connector.TmsType = dto.TmsType;
        connector.BaseUrl = dto.BaseUrl;
        connector.HttpMethod = dto.HttpMethod;
        connector.RequestContentType = dto.RequestContentType;
        connector.ResponseFormat = dto.ResponseFormat;
        connector.AuthType = dto.AuthType;
        connector.AuthHeaderName = dto.AuthHeaderName;
        connector.AuthUsername = dto.AuthUsername;
        connector.AuthTokenUrl = dto.AuthTokenUrl;
        connector.AuthTokenResponsePath = dto.AuthTokenResponsePath;
        connector.IsActive = dto.IsActive;
        connector.SampleRequestPayload = dto.SampleRequestPayload;
        connector.SampleResponsePayload = dto.SampleResponsePayload;

        if (!string.IsNullOrWhiteSpace(dto.AuthApiKey))
            connector.AuthApiKey = dto.AuthApiKey;
        if (!string.IsNullOrWhiteSpace(dto.AuthPassword))
            connector.AuthPassword = dto.AuthPassword;
    }

    private static void ReplaceMappings(ShippingConnector connector, List<UpsertFieldMappingDto> mappings)
    {
        connector.FieldMappings.Clear();
        foreach (var m in mappings)
        {
            connector.FieldMappings.Add(new ShippingConnectorFieldMapping
            {
                Direction = m.Direction,
                NovaField = m.NovaField,
                ExternalPath = m.ExternalPath,
                Transform = string.IsNullOrWhiteSpace(m.Transform) ? "None" : m.Transform,
                ConstantValue = m.ConstantValue
            });
        }
    }

    private static ShippingConnectorDto ToDto(ShippingConnector c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        TmsType = c.TmsType,
        BaseUrl = c.BaseUrl,
        HttpMethod = c.HttpMethod,
        RequestContentType = c.RequestContentType,
        ResponseFormat = c.ResponseFormat,
        AuthType = c.AuthType,
        AuthHeaderName = c.AuthHeaderName,
        AuthUsername = c.AuthUsername,
        AuthTokenUrl = c.AuthTokenUrl,
        AuthTokenResponsePath = c.AuthTokenResponsePath,
        HasAuthApiKey = !string.IsNullOrWhiteSpace(c.AuthApiKey),
        HasAuthPassword = !string.IsNullOrWhiteSpace(c.AuthPassword),
        IsActive = c.IsActive,
        SampleRequestPayload = c.SampleRequestPayload,
        SampleResponsePayload = c.SampleResponsePayload,
        FieldMappings = c.FieldMappings.Select(m => new ShippingConnectorFieldMappingDto
        {
            Id = m.Id,
            Direction = m.Direction,
            NovaField = m.NovaField,
            ExternalPath = m.ExternalPath,
            Transform = m.Transform,
            ConstantValue = m.ConstantValue
        }).ToList()
    };
}
