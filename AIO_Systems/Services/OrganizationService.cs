using System.Text;
using System.Text.RegularExpressions;
using AIO_Systems.Data;
using AIO_Systems.Domain.Entities;
using AIO_Systems.DTOs.Organization;
using AIO_Systems.Services.Interfaces;
using AIO_Systems.Shared;
using Microsoft.EntityFrameworkCore;

namespace AIO_Systems.Services;

public partial class OrganizationService(AppDbContext db, IPasswordService passwordService) : IOrganizationService
{
    public async Task<IEnumerable<OrganizationDto>> GetAllAsync()
    {
        var orgs = await db.Organizations.ToListAsync();
        var dtos = new List<OrganizationDto>();
        foreach (var org in orgs)
        {
            var userCount = await db.Users.CountAsync(u => u.OrganizationId == org.Id);
            dtos.Add(ToDto(org, userCount));
        }
        return dtos;
    }

    public async Task<OrganizationDto?> GetByIdAsync(int id)
    {
        var org = await db.Organizations.FindAsync(id);
        if (org == null) return null;
        var userCount = await db.Users.CountAsync(u => u.OrganizationId == id);
        return ToDto(org, userCount);
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request)
    {
        var org = new Organization
        {
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            LogoUrl = request.LogoUrl,
            Timezone = request.Timezone,
            Currency = request.Currency,
            IsActive = true,
            TenantKey = GenerateTenantKey(request.Name),
        };

        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        return await GetByIdAsync(org.Id) ?? throw new Exception("Organization not found after create");
    }

    public async Task<OrganizationDto> UpdateAsync(int id, UpdateOrganizationRequest request)
    {
        var org = await db.Organizations.FindAsync(id) ?? throw new KeyNotFoundException($"Organization {id} not found");

        org.Name = request.Name;
        org.Address = request.Address;
        org.Phone = request.Phone;
        org.Email = request.Email;
        org.LogoUrl = request.LogoUrl;
        org.Timezone = request.Timezone;
        org.Currency = request.Currency;
        org.IsActive = request.IsActive;

        await db.SaveChangesAsync();
        return await GetByIdAsync(id) ?? throw new Exception("Organization not found after update");
    }

    public async Task DeleteAsync(int id)
    {
        var org = await db.Organizations.FindAsync(id);
        if (org == null) return;
        db.Organizations.Remove(org);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrgUserDto>> GetOrgUsersAsync(int organizationId)
    {
        var users = await db.Users
            .Include(u => u.Role)
            .Where(u => u.OrganizationId == organizationId)
            .ToListAsync();

        return users.Select(u => new OrgUserDto
        {
            Id = u.Id, Name = u.Name, Email = u.Email,
            RoleName = u.Role.RoleName, IsActive = u.IsActive, CreatedDate = u.CreatedDate,
        });
    }

    public async Task<OrgUserDto> CreateOrgAdminAsync(int organizationId, CreateOrgAdminRequest request)
    {
        var org = await db.Organizations.FindAsync(organizationId)
            ?? throw new KeyNotFoundException($"Organization {organizationId} not found");

        if (await db.Users.AnyAsync(u => u.Email == request.Email))
            throw new AppException("A user with this email already exists.", statusCode: 409);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordService.HashPassword(request.Password),
            RoleId = 2,  // Admin role
            OrganizationId = organizationId,
            IsActive = true,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var role = await db.Roles.FindAsync(user.RoleId);

        return new OrgUserDto
        {
            Id = user.Id, Name = user.Name, Email = user.Email,
            RoleName = role!.RoleName, IsActive = user.IsActive, CreatedDate = user.CreatedDate,
        };
    }

    // ── License methods ─────────────────────────────────────────────────

    public async Task<IEnumerable<LicenseDto>> GetAllLicensesAsync()
    {
        var licenses = await db.Licenses.Include(l => l.Organization).ToListAsync();
        return licenses.Select(ToLicenseDto);
    }

    public async Task<LicenseDto?> GetLicenseByOrgAsync(int orgId)
    {
        var license = await db.Licenses.Include(l => l.Organization)
            .FirstOrDefaultAsync(l => l.OrganizationId == orgId);
        return license == null ? null : ToLicenseDto(license);
    }

    public async Task<LicenseDto> UpsertLicenseAsync(int orgId, UpsertLicenseRequest request)
    {
        var license = await db.Licenses.Include(l => l.Organization)
            .FirstOrDefaultAsync(l => l.OrganizationId == orgId);

        if (license == null)
        {
            license = new License { OrganizationId = orgId };
            db.Licenses.Add(license);
        }

        license.Plan = request.Plan;
        license.Status = request.Status;
        license.ExpiryDate = request.ExpiryDate;
        license.MaxUsers = request.MaxUsers;
        license.Notes = request.Notes;

        await db.SaveChangesAsync();

        if (license.Organization == null)
            license.Organization = await db.Organizations.FindAsync(orgId) ?? throw new KeyNotFoundException($"Organization {orgId} not found");

        return ToLicenseDto(license);
    }

    public async Task DeleteLicenseAsync(int orgId)
    {
        var license = await db.Licenses.FirstOrDefaultAsync(l => l.OrganizationId == orgId);
        if (license == null) return;
        db.Licenses.Remove(license);
        await db.SaveChangesAsync();
    }

    private static OrganizationDto ToDto(Organization org, int userCount) => new()
    {
        Id = org.Id, Name = org.Name, Address = org.Address, Phone = org.Phone,
        Email = org.Email, LogoUrl = org.LogoUrl, IsActive = org.IsActive,
        Timezone = org.Timezone, Currency = org.Currency, TenantKey = org.TenantKey,
        UserCount = userCount, CreatedDate = org.CreatedDate,
    };

    private static LicenseDto ToLicenseDto(License license) => new()
    {
        Id             = license.Id,
        OrganizationId = license.OrganizationId,
        OrgName        = license.Organization.Name,
        Plan           = license.Plan,
        Status         = license.Status,
        ExpiryDate     = license.ExpiryDate,
        MaxUsers       = license.MaxUsers,
        Notes          = license.Notes,
        CreatedDate    = license.CreatedDate,
    };

    /// <summary>
    /// Slugifies the organization name (lowercase, non-alphanumeric -&gt; '-', trim/collapse dashes)
    /// and appends a short random suffix to guarantee uniqueness without needing the Id first.
    /// </summary>
    private static string GenerateTenantKey(string name)
    {
        var slug = SlugRegex().Replace(name.ToLowerInvariant(), "-").Trim('-');
        slug = CollapseDashesRegex().Replace(slug, "-");
        if (string.IsNullOrWhiteSpace(slug))
            slug = "org";

        var suffix = Guid.NewGuid().ToString("N")[..6];
        return $"{slug}-{suffix}";
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex CollapseDashesRegex();
}
