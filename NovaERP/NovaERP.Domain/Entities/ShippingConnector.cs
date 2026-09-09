namespace NovaERP.Domain.Entities;

/// <summary>An org-configured, mapping-driven integration to an external TMS/rate-shop HTTP
/// API. No per-vendor code is required to onboard a new TMS — the request is built and the
/// response is parsed entirely from the FieldMappings below (see JsonPathWalker /
/// ShippingRateService), keyed off a pasted sample payload rather than a hardcoded schema.
/// "Rate shop" means querying every active connector for an org and merging the results, so
/// several rows can coexist.</summary>
public class ShippingConnector : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string Name           { get; set; } = string.Empty;
    public string TmsType        { get; set; } = string.Empty; // freeform label only, e.g. "Piyovi"

    public string BaseUrl    { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "POST"; // POST | GET

    /// <summary>How the outbound request body is built. Json (default) builds a nested JSON
    /// object from Outbound mappings. FormUrlEncoded builds flat "key=value" pairs instead —
    /// some carriers (e.g. legacy TMS rate-quote APIs) take application/x-www-form-urlencoded
    /// with per-package fields addressed as "wweight[1]", "wweight[2]", etc. Xml/Text send the
    /// Sample Request Payload verbatim (no mapping-driven building) — used for the Postman-style
    /// Send Test Request panel to try literally any request shape before deciding whether full
    /// mapping support is worth adding for that shape.</summary>
    public string RequestContentType { get; set; } = "Json"; // Json | FormUrlEncoded | Xml | Text

    /// <summary>How the response is parsed for Inbound mappings. Json (default) parses the body
    /// as JSON; Xml parses it as XML, addressed with the same dot/bracket path convention via
    /// XmlPathWalker (repeated sibling elements are this format's "array").</summary>
    public string ResponseFormat { get; set; } = "Json"; // Json | Xml

    public string  AuthType     { get; set; } = "None"; // None | ApiKeyHeader | BearerToken | BasicAuth | TokenLogin
    public string? AuthHeaderName { get; set; } // used when AuthType = ApiKeyHeader
    public string? AuthApiKey     { get; set; } // used when AuthType = ApiKeyHeader | BearerToken
    public string? AuthUsername   { get; set; } // used when AuthType = BasicAuth | TokenLogin (as "principal")
    public string? AuthPassword   { get; set; } // used when AuthType = BasicAuth | TokenLogin (as "credential")

    /// <summary>TokenLogin only: a separate endpoint called first with
    /// {"principal": AuthUsername, "credential": AuthPassword} — the resulting token is then
    /// sent as a Bearer token on the actual request. Covers carriers (e.g. eShipper) whose auth
    /// is a login call rather than a static key.</summary>
    public string? AuthTokenUrl { get; set; }
    /// <summary>TokenLogin only: dot/bracket path to the token field in the login response
    /// (e.g. "token", "access_token") — defaults to "token" when blank.</summary>
    public string? AuthTokenResponsePath { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Kept only so the mapping editor can be re-opened later with the same detected
    /// field list — never read at runtime.</summary>
    public string? SampleRequestPayload  { get; set; }
    public string? SampleResponsePayload { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<ShippingConnectorFieldMapping> FieldMappings { get; set; } = new List<ShippingConnectorFieldMapping>();
}
