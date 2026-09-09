using System.Text.Json.Serialization;

namespace LinkShield.Infrastructure.Analysis;

/// <summary>Minimal subset of the RDAP domain response (RFC 9083) actually used here.</summary>
public class RdapDomainResponse
{
    [JsonPropertyName("ldhName")] public string? LdhName { get; set; }
    [JsonPropertyName("events")] public List<RdapEvent>? Events { get; set; }
    [JsonPropertyName("entities")] public List<RdapEntity>? Entities { get; set; }
    [JsonPropertyName("nameservers")] public List<RdapNameserver>? Nameservers { get; set; }
}

public class RdapEvent
{
    [JsonPropertyName("eventAction")] public string? EventAction { get; set; }
    [JsonPropertyName("eventDate")] public DateTime? EventDate { get; set; }
}

public class RdapEntity
{
    [JsonPropertyName("roles")] public List<string>? Roles { get; set; }
    [JsonPropertyName("vcardArray")] public System.Text.Json.JsonElement? VcardArray { get; set; }
}

public class RdapNameserver
{
    [JsonPropertyName("ldhName")] public string? LdhName { get; set; }
}
