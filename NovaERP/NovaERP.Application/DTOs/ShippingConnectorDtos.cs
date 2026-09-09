namespace NovaERP.Application.DTOs;

public class ShippingConnectorFieldMappingDto
{
    public int     Id            { get; set; }
    public string  Direction     { get; set; } = string.Empty; // Outbound | Inbound
    public string  NovaField     { get; set; } = string.Empty;
    public string  ExternalPath  { get; set; } = string.Empty;
    public string  Transform     { get; set; } = "None";
    public string? ConstantValue { get; set; }
}

public class UpsertFieldMappingDto
{
    /// <summary>Null means "add a new row" — existing rows not present in the submitted list
    /// are removed, matching the "replace wholesale" convention used for order-line
    /// collections elsewhere in this codebase.</summary>
    public int?    Id            { get; set; }
    public string  Direction     { get; set; } = string.Empty;
    public string  NovaField     { get; set; } = string.Empty;
    public string  ExternalPath  { get; set; } = string.Empty;
    public string  Transform     { get; set; } = "None";
    public string? ConstantValue { get; set; }
}

public class ShippingConnectorDto
{
    public int    Id      { get; set; }
    public string Name    { get; set; } = string.Empty;
    public string TmsType { get; set; } = string.Empty;

    public string BaseUrl    { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "POST";
    public string RequestContentType { get; set; } = "Json";
    public string ResponseFormat     { get; set; } = "Json";

    public string  AuthType       { get; set; } = "None";
    public string? AuthHeaderName { get; set; }
    public string? AuthUsername   { get; set; }
    public string? AuthTokenUrl { get; set; }
    public string? AuthTokenResponsePath { get; set; }
    /// <summary>Never the raw secret — just whether one is currently set, same convention as
    /// AiAssistantSettingsDto.IsConfigured.</summary>
    public bool HasAuthApiKey  { get; set; }
    public bool HasAuthPassword { get; set; }

    public bool IsActive { get; set; }

    public string? SampleRequestPayload  { get; set; }
    public string? SampleResponsePayload { get; set; }

    public List<ShippingConnectorFieldMappingDto> FieldMappings { get; set; } = new();
}

public class SaveShippingConnectorDto
{
    public string Name    { get; set; } = string.Empty;
    public string TmsType { get; set; } = string.Empty;

    public string BaseUrl    { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "POST";
    public string RequestContentType { get; set; } = "Json";
    public string ResponseFormat     { get; set; } = "Json";

    public string  AuthType       { get; set; } = "None";
    public string? AuthHeaderName { get; set; }
    /// <summary>Blank means "keep the existing key unchanged" on update — same convention as
    /// AiAssistantSettings/NotificationChannelSettings.</summary>
    public string? AuthApiKey   { get; set; }
    public string? AuthUsername { get; set; }
    public string? AuthPassword { get; set; }
    public string? AuthTokenUrl { get; set; }
    public string? AuthTokenResponsePath { get; set; }

    public bool IsActive { get; set; } = true;

    public string? SampleRequestPayload  { get; set; }
    public string? SampleResponsePayload { get; set; }

    public List<UpsertFieldMappingDto> FieldMappings { get; set; } = new();
}

public class RateSurchargeDto
{
    public string?  Name   { get; set; }
    public decimal? Amount { get; set; }
}

public class RateQuoteDto
{
    public int     ConnectorId   { get; set; }
    public string  ConnectorName { get; set; } = string.Empty;
    public string? CarrierName   { get; set; }
    public string? ServiceLevel  { get; set; }
    public decimal? PublishedCost { get; set; }
    public decimal? Price        { get; set; }
    public string?  Currency     { get; set; }
    public int?     EstimatedDays { get; set; }
    public DateTime? Etd         { get; set; }
    public string?  RateId       { get; set; }
    /// <summary>Optional per-quote charge breakdown (e.g. eShipper's "surcharges": [{name,
    /// amount}]) — populated when the connector maps Quote.SurchargeName/SurchargeAmount.
    /// Empty when the carrier's response has no such breakdown or it isn't mapped.</summary>
    public List<RateSurchargeDto> Surcharges { get; set; } = new();
}

public class RateQuoteResultDto
{
    public List<RateQuoteDto> Quotes { get; set; } = new();
    /// <summary>One entry per connector that failed (bad URL, non-2xx, malformed JSON) — never
    /// blocks quotes from other connectors succeeding.</summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>Result of running a single connector — returned by IShippingConnectorRunner and
/// merged across connectors by ShippingRateService.</summary>
public class ConnectorRunResultDto
{
    public List<RateQuoteDto> Quotes { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>Postman-style "send it now" test: the whole draft connector form (so an
/// unsaved-yet connector can be tried before Save) plus the exact request body to send.</summary>
public class TestShippingConnectorDto
{
    public SaveShippingConnectorDto Connector { get; set; } = new();
    public string? RequestBody { get; set; }
}

/// <summary>Raw HTTP result of a test send — the actual status/body from the external API,
/// not parsed/mapped, so the user can see exactly what came back before building mappings.</summary>
public class ConnectorTestResultDto
{
    public bool    Success      { get; set; }
    public int?    StatusCode   { get; set; }
    public string? ResponseBody { get; set; }
    public string? Error        { get; set; }
    public long    ElapsedMs    { get; set; }
}
