using LinkShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Persistence;

public class LinkShieldDbContext : DbContext
{
    public LinkShieldDbContext(DbContextOptions<LinkShieldDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<UrlScan> UrlScans => Set<UrlScan>();
    public DbSet<UrlAnalysis> UrlAnalyses => Set<UrlAnalysis>();
    public DbSet<DomainAnalysis> DomainAnalyses => Set<DomainAnalysis>();
    public DbSet<DnsAnalysis> DnsAnalyses => Set<DnsAnalysis>();
    public DbSet<SslAnalysis> SslAnalyses => Set<SslAnalysis>();
    public DbSet<RedirectAnalysis> RedirectAnalyses => Set<RedirectAnalysis>();

    public DbSet<ThreatIntelligenceProvider> ThreatIntelligenceProviders => Set<ThreatIntelligenceProvider>();
    public DbSet<ThreatIntelligenceResult> ThreatIntelligenceResults => Set<ThreatIntelligenceResult>();

    public DbSet<BrandProfile> BrandProfiles => Set<BrandProfile>();
    public DbSet<BrandMatch> BrandMatches => Set<BrandMatch>();

    public DbSet<RiskRule> RiskRules => Set<RiskRule>();
    public DbSet<RiskFactor> RiskFactors => Set<RiskFactor>();

    public DbSet<MlPrediction> MlPredictions => Set<MlPrediction>();
    public DbSet<ScanEvent> ScanEvents => Set<ScanEvent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<ApiClient> ApiClients => Set<ApiClient>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LinkShieldDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Domain.Common.BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static System.Linq.Expressions.LambdaExpression BuildSoftDeleteFilter(Type clrType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(clrType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(Domain.Common.BaseEntity.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }
}
