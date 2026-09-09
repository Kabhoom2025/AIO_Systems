using System.Text.Json;
using LinkShield.Application.DTOs.Admin;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Entities;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Services;

public class BrandManagementService : IBrandManagementService
{
    private readonly LinkShieldDbContext _db;

    public BrandManagementService(LinkShieldDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BrandProfileDto>> GetAllAsync(CancellationToken ct = default) =>
        await _db.BrandProfiles
            .OrderBy(b => b.BrandName)
            .Select(b => new BrandProfileDto(b.Id, b.BrandName, b.OfficialDomain, DeserializeAliases(b.AliasDomainsJson), b.IsEnabled))
            .ToListAsync(ct);

    public async Task<BrandProfileDto> CreateAsync(CreateBrandProfileRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        if (await _db.BrandProfiles.AnyAsync(b => b.OfficialDomain == request.OfficialDomain, ct))
            throw new InvalidOperationException($"A brand profile for '{request.OfficialDomain}' already exists.");

        var brand = new BrandProfile
        {
            BrandName = request.BrandName,
            OfficialDomain = request.OfficialDomain,
            AliasDomainsJson = JsonSerializer.Serialize(request.AliasDomains ?? [])
        };
        _db.BrandProfiles.Add(brand);

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = actingUserId,
            Action = AuditAction.BrandProfileChanged,
            EntityType = nameof(BrandProfile),
            EntityId = brand.Id.ToString(),
            DetailsJson = JsonSerializer.Serialize(new { action = "created", brand.BrandName, brand.OfficialDomain })
        });

        await _db.SaveChangesAsync(ct);

        return new BrandProfileDto(brand.Id, brand.BrandName, brand.OfficialDomain, request.AliasDomains ?? [], brand.IsEnabled);
    }

    private static IReadOnlyList<string> DeserializeAliases(string json) =>
        JsonSerializer.Deserialize<List<string>>(json) ?? [];
}
