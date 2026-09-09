using FlowSphere.Domain.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Infrastructure.Authentication;
using FlowSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlowSphere.Infrastructure.Seed;

/// <summary>Seeds on first startup only (mirrors every sibling service's "if (!db.X.Any())"
/// convention): one demo Organization, one Admin Role with the full permission catalog, and
/// one Admin User.</summary>
public class DemoDataSeeder
{
    private readonly FlowSphereDbContext _db;
    private readonly PasswordHasher _passwordHasher;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(FlowSphereDbContext db, PasswordHasher passwordHasher, ILogger<DemoDataSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await AnyOrganizationsAsync())
        {
            return;
        }

        _logger.LogInformation("Seeding FlowSphereAI demo data...");

        var organization = new Organization { Name = "FlowSphere Demo Org", IsActive = true };
        _db.Organizations.Add(organization);
        await _db.SaveChangesAsync();

        var adminRole = new Role
        {
            OrganizationId = organization.Id,
            Name = "Admin",
            Permissions = string.Join(',', PermissionCatalog.All)
        };
        _db.Roles.Add(adminRole);
        await _db.SaveChangesAsync();

        var adminUser = new User
        {
            OrganizationId = organization.Id,
            Name = "FlowSphere Admin",
            Email = "admin@flowsphere.demo",
            PasswordHash = _passwordHasher.Hash("Admin@123"),
            RoleId = adminRole.Id,
            IsActive = true,
            IsEmailVerified = true,
        };
        _db.Users.Add(adminUser);
        await _db.SaveChangesAsync();

        // Platform-provided built-in connectors (OrganizationId = null = shared across every
        // tenant). Credentials are added per-tenant later via ConnectorsController, not here.
        _db.Connectors.AddRange(
            new Connector { Type = "Http", Name = "HTTP / REST", IsEnabled = true },
            new Connector { Type = "Smtp", Name = "Email (SMTP)", IsEnabled = true },
            new Connector { Type = "OpenAi", Name = "OpenAI", IsEnabled = true },
            new Connector { Type = "Slack", Name = "Slack", IsEnabled = true },
            new Connector { Type = "Teams", Name = "Microsoft Teams", IsEnabled = true },
            new Connector { Type = "Discord", Name = "Discord", IsEnabled = true },
            new Connector { Type = "Webhook", Name = "Webhook / Custom", IsEnabled = true },
            new Connector { Type = "Twilio", Name = "Twilio (SMS)", IsEnabled = true },
            new Connector { Type = "Notion", Name = "Notion", IsEnabled = true },
            new Connector { Type = "Jira", Name = "Jira", IsEnabled = true },
            new Connector { Type = "Airtable", Name = "Airtable", IsEnabled = true },
            new Connector { Type = "GoogleSheets", Name = "Google Sheets", IsEnabled = true });
        await _db.SaveChangesAsync();

        _logger.LogInformation("FlowSphereAI demo data seeded (admin@flowsphere.demo / Admin@123).");
    }

    private async Task<bool> AnyOrganizationsAsync()
    {
        // IgnoreQueryFilters: at seed time there is no authenticated caller, so the tenant
        // query filter (which reads OrganizationId from a not-yet-existent JWT) must be bypassed.
        return await _db.Organizations.IgnoreQueryFilters().AnyAsync();
    }
}
