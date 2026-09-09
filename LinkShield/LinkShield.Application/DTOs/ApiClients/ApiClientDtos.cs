namespace LinkShield.Application.DTOs.ApiClients;

public record CreateApiClientRequest(string OrganizationName, string ContactEmail, int? DailyQuota, int? RequestsPerMinute);

public record ApiClientDto(
    Guid Id, string OrganizationName, string ContactEmail, bool IsActive,
    int DailyQuota, int RequestsPerMinute, IReadOnlyList<ApiKeyDto> Keys);

public record ApiKeyDto(
    Guid Id, string KeyPrefix, string Label, DateTime? ExpiresAtUtc,
    DateTime? RevokedAtUtc, DateTime? LastUsedAtUtc, long UsageCount, bool IsActive);

public record CreateApiKeyRequest(string Label, DateTime? ExpiresAtUtc);

/// <summary>The raw key is only ever returned once, at creation time — it is never
/// recoverable afterward (only KeyHash is stored). Same convention as GitHub PATs etc.</summary>
public record CreatedApiKeyDto(Guid Id, string RawKey, string KeyPrefix, string Label);

public record ExternalUrlCheckRequest(string Url);
