using LinkShield.Application.DTOs.ApiClients;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Entities;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.ApiKeyAuth;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Services;

public class ApiClientService : IApiClientService
{
    private const int DefaultDailyQuota = 1000;
    private const int DefaultRequestsPerMinute = 60;

    private readonly LinkShieldDbContext _db;

    public ApiClientService(LinkShieldDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ApiClientDto>> GetAllAsync(CancellationToken ct = default)
    {
        var clients = await _db.ApiClients
            .Include(c => c.ApiKeys)
            .OrderBy(c => c.OrganizationName)
            .ToListAsync(ct);

        return clients.Select(ToDto).ToList();
    }

    public async Task<ApiClientDto> CreateAsync(CreateApiClientRequest request, CancellationToken ct = default)
    {
        var client = new ApiClient
        {
            OrganizationName = request.OrganizationName,
            ContactEmail = request.ContactEmail,
            DailyQuota = request.DailyQuota ?? DefaultDailyQuota,
            RequestsPerMinute = request.RequestsPerMinute ?? DefaultRequestsPerMinute
        };
        _db.ApiClients.Add(client);
        await _db.SaveChangesAsync(ct);

        return ToDto(client);
    }

    public async Task<CreatedApiKeyDto> CreateKeyAsync(Guid apiClientId, CreateApiKeyRequest request, CancellationToken ct = default)
    {
        var client = await _db.ApiClients.FirstOrDefaultAsync(c => c.Id == apiClientId, ct)
            ?? throw new KeyNotFoundException($"API client '{apiClientId}' not found.");

        var (rawKey, keyPrefix, keyHash) = ApiKeyHasher.GenerateNewKey();
        var apiKey = new ApiKey
        {
            ApiClientId = client.Id,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            Label = request.Label,
            ExpiresAtUtc = request.ExpiresAtUtc
        };
        _db.ApiKeys.Add(apiKey);

        _db.AuditLogs.Add(new AuditLog
        {
            Action = AuditAction.ApiKeyCreated, EntityType = nameof(ApiKey), EntityId = apiKey.Id.ToString(),
            DetailsJson = System.Text.Json.JsonSerializer.Serialize(new { client.OrganizationName, apiKey.Label, apiKey.KeyPrefix })
        });

        await _db.SaveChangesAsync(ct);

        return new CreatedApiKeyDto(apiKey.Id, rawKey, keyPrefix, apiKey.Label);
    }

    public async Task<bool> RevokeKeyAsync(Guid apiKeyId, CancellationToken ct = default)
    {
        var apiKey = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == apiKeyId, ct);
        if (apiKey is null || apiKey.RevokedAtUtc.HasValue) return false;

        apiKey.RevokedAtUtc = DateTime.UtcNow;
        _db.AuditLogs.Add(new AuditLog { Action = AuditAction.ApiKeyRevoked, EntityType = nameof(ApiKey), EntityId = apiKey.Id.ToString() });
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ApiClientDto ToDto(ApiClient client) => new(
        client.Id, client.OrganizationName, client.ContactEmail, client.IsActive,
        client.DailyQuota, client.RequestsPerMinute,
        client.ApiKeys.Select(k => new ApiKeyDto(
            k.Id, k.KeyPrefix, k.Label, k.ExpiresAtUtc, k.RevokedAtUtc, k.LastUsedAtUtc, k.UsageCount, k.IsActive)).ToList());
}
