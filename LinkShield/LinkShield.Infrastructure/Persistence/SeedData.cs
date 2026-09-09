using LinkShield.Domain.Entities;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Persistence;

/// <summary>
/// Reference data every environment needs to function (roles, threat-intel provider
/// registrations, brand profiles, default risk rules) plus one demo SuperAdmin login —
/// the same "seed a demo account" convention every other AIO_Systems vertical follows
/// (see NovaERP/HRMS/etc.), needed here so the admin panel has something to log in with.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(LinkShieldDbContext db)
    {
        List<Role> roles;
        if (!await db.Roles.AnyAsync())
        {
            roles = Enum.GetValues<SystemRole>().Select(r => new Role { Name = r, Description = $"{r} system role" }).ToList();
            db.Roles.AddRange(roles);
        }
        else
        {
            roles = await db.Roles.ToListAsync();
        }

        if (!await db.ThreatIntelligenceProviders.AnyAsync())
        {
            db.ThreatIntelligenceProviders.AddRange(
                new ThreatIntelligenceProvider
                {
                    Name = "Google Safe Browsing",
                    Slug = "google-safe-browsing",
                    BaseUrl = "https://safebrowsing.googleapis.com",
                    ApiKeySecretName = "ThreatIntel:GoogleSafeBrowsing:ApiKey",
                    WeightMultiplier = 1.0m
                },
                new ThreatIntelligenceProvider
                {
                    Name = "VirusTotal",
                    Slug = "virustotal",
                    BaseUrl = "https://www.virustotal.com/api/v3",
                    ApiKeySecretName = "ThreatIntel:VirusTotal:ApiKey",
                    WeightMultiplier = 1.0m
                },
                new ThreatIntelligenceProvider
                {
                    Name = "URLhaus",
                    Slug = "urlhaus",
                    BaseUrl = "https://urlhaus-api.abuse.ch",
                    ApiKeySecretName = "ThreatIntel:URLhaus:ApiKey",
                    WeightMultiplier = 0.8m
                },
                new ThreatIntelligenceProvider
                {
                    Name = "PhishTank",
                    Slug = "phishtank",
                    BaseUrl = "https://checkurl.phishtank.com",
                    ApiKeySecretName = "ThreatIntel:PhishTank:ApiKey",
                    WeightMultiplier = 0.8m
                });
        }

        if (!await db.BrandProfiles.AnyAsync())
        {
            db.BrandProfiles.AddRange(
                new BrandProfile { BrandName = "Microsoft", OfficialDomain = "microsoft.com" },
                new BrandProfile { BrandName = "PayPal", OfficialDomain = "paypal.com" },
                new BrandProfile { BrandName = "Amazon", OfficialDomain = "amazon.com" },
                new BrandProfile { BrandName = "Apple", OfficialDomain = "apple.com" },
                new BrandProfile { BrandName = "Google", OfficialDomain = "google.com" });
        }

        if (!await db.RiskRules.AnyAsync())
        {
            db.RiskRules.AddRange(
                new RiskRule { Name = "ThreatIntelligenceMatch", Category = RiskFactorCategory.ThreatIntelligence, Weight = 30m, ScoreContribution = 40, ConditionExpression = "ThreatIntel.AnyConfirmedMalicious" },
                new RiskRule { Name = "BrandImpersonation", Category = RiskFactorCategory.BrandImpersonation, Weight = 15m, ScoreContribution = 20, ConditionExpression = "Brand.HasMatch" },
                new RiskRule { Name = "VeryNewDomain", Category = RiskFactorCategory.DomainIntelligence, Weight = 15m, ScoreContribution = 10, ConditionExpression = "Domain.AgeDays < 30" },
                new RiskRule { Name = "MultipleRedirects", Category = RiskFactorCategory.RedirectAnalysis, Weight = 10m, ScoreContribution = 8, ConditionExpression = "Redirects.Count > 3" },
                new RiskRule { Name = "SuspiciousAuthKeyword", Category = RiskFactorCategory.UrlAnalysis, Weight = 15m, ScoreContribution = 6, ConditionExpression = "Url.HasAuthKeyword" },
                new RiskRule { Name = "IpAddressHost", Category = RiskFactorCategory.UrlAnalysis, Weight = 15m, ScoreContribution = 12, ConditionExpression = "Url.IsIpAddressHost" },
                new RiskRule { Name = "ExpiredOrInvalidCertificate", Category = RiskFactorCategory.SslAnalysis, Weight = 5m, ScoreContribution = 8, ConditionExpression = "Ssl.IsExpired || !Ssl.IsValid" },
                new RiskRule { Name = "NoMxRecord", Category = RiskFactorCategory.DnsAnalysis, Weight = 5m, ScoreContribution = 3, ConditionExpression = "!Dns.HasMailConfiguration" });
        }

        if (!await db.SystemSettings.AnyAsync())
        {
            db.SystemSettings.AddRange(
                new SystemSetting { Key = "RiskEngine:LowThreshold", Value = "20", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:ModerateThreshold", Value = "40", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:HighThreshold", Value = "70", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:Weight:ThreatIntelligence", Value = "0.30", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:Weight:DomainIntelligence", Value = "0.15", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:Weight:UrlAnalysis", Value = "0.15", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:Weight:BrandDetection", Value = "0.15", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:Weight:RedirectAnalysis", Value = "0.10", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:Weight:DnsAnalysis", Value = "0.05", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:Weight:SslAnalysis", Value = "0.05", Category = "RiskEngine" },
                new SystemSetting { Key = "RiskEngine:Weight:MachineLearning", Value = "0.05", Category = "RiskEngine" });
        }

        const string demoAdminEmail = "admin@linkshield.local";
        if (!await db.Users.AnyAsync(u => u.Email == demoAdminEmail))
        {
            var superAdminRole = roles.First(r => r.Name == SystemRole.SuperAdmin);
            var adminUser = new User
            {
                Email = demoAdminEmail,
                PasswordHash = PasswordHasher.Hash("Admin@123"),
                FirstName = "Super",
                LastName = "Admin",
                EmailVerified = true,
                IsActive = true
            };
            db.Users.Add(adminUser);
            db.UserRoles.Add(new UserRole { User = adminUser, Role = superAdminRole });
        }

        await db.SaveChangesAsync();
    }
}
