using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Infrastructure.Authentication;

/// <summary>Phase 1 credential storage: IDataProtector (DPAPI-backed on Windows, key-ring-backed
/// cross-platform) encrypts values at rest. Vault/KMS integration is deferred to a later phase.</summary>
public class ConnectorCredentialStore : IConnectorCredentialStore
{
    private const string ProtectorPurpose = "FlowSphere.ConnectorCredentials";

    private readonly FlowSphereDbContext _db;
    private readonly IDataProtector _protector;

    public ConnectorCredentialStore(FlowSphereDbContext db, IDataProtectionProvider dataProtectionProvider)
    {
        _db = db;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    }

    public async Task<string?> GetAsync(int connectorId, string keyName, int organizationId, CancellationToken cancellationToken)
    {
        // Built-in connectors (Http/Smtp/OpenAi) are shared rows with the same ConnectorId across
        // every tenant, so the organizationId filter here is load-bearing, not defensive - without
        // it this would return whichever tenant's credential happened to be inserted first.
        //
        // IgnoreQueryFilters is required too: ConnectorCredential implements ITenantScoped, so
        // the DbContext's global query filter also ANDs in `OrganizationId == currentUser.OrganizationId`.
        // That's correct during an HTTP request, but node executors call this from the background
        // execution dispatcher, which has no HTTP context - ICurrentUserContext.OrganizationId
        // silently resolves to 0 there, and the global filter would then hide every real
        // credential regardless of the explicit, correct `organizationId` parameter below.
        var credential = await _db.ConnectorCredentials
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.ConnectorId == connectorId && c.KeyName == keyName && c.OrganizationId == organizationId, cancellationToken);

        return credential is null ? null : _protector.Unprotect(credential.EncryptedValue);
    }

    public async Task SetAsync(
        int connectorId, string keyName, string plainValue, int organizationId, int createdByUserId, CancellationToken cancellationToken)
    {
        var encrypted = _protector.Protect(plainValue);

        // Credentials are always scoped to the calling tenant (organizationId), even for shared
        // platform connectors whose Connector.OrganizationId is null - a built-in Http/Smtp/OpenAi
        // connector's *credentials* are still per-tenant secrets, not shared across tenants.
        // IgnoreQueryFilters: see GetAsync above - same reasoning applies here.
        var existing = await _db.ConnectorCredentials
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.ConnectorId == connectorId && c.KeyName == keyName && c.OrganizationId == organizationId, cancellationToken);

        if (existing is not null)
        {
            existing.EncryptedValue = encrypted;
        }
        else
        {
            _db.ConnectorCredentials.Add(new ConnectorCredential
            {
                ConnectorId = connectorId,
                KeyName = keyName,
                EncryptedValue = encrypted,
                OrganizationId = organizationId,
                CreatedByUserId = createdByUserId,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
