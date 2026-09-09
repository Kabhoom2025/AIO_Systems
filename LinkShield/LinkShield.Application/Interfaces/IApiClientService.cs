using LinkShield.Application.DTOs.ApiClients;

namespace LinkShield.Application.Interfaces;

public interface IApiClientService
{
    Task<IReadOnlyList<ApiClientDto>> GetAllAsync(CancellationToken ct = default);
    Task<ApiClientDto> CreateAsync(CreateApiClientRequest request, CancellationToken ct = default);
    Task<CreatedApiKeyDto> CreateKeyAsync(Guid apiClientId, CreateApiKeyRequest request, CancellationToken ct = default);
    Task<bool> RevokeKeyAsync(Guid apiKeyId, CancellationToken ct = default);
}

public record ApiKeyValidationResult(bool IsValid, Guid? ApiClientId, string? OrganizationName, string? DenyReason);

/// <summary>Validates an incoming X-API-Key header value: looks up the key by hash, checks
/// active/expiry/client-active, enforces the client's daily quota, and records usage. Kept
/// separate from IApiClientService (that's admin CRUD; this is the hot request-time path).</summary>
public interface IApiKeyValidator
{
    Task<ApiKeyValidationResult> ValidateAndRecordUsageAsync(string rawKey, CancellationToken ct = default);
}
