using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence;

/// <summary>
/// Owns only platform metadata (platform.* tables). It never touches the generated application
/// tables (app_data.*) - those are created/queried through raw, parameterized DDL/DML in
/// Platform.Runtime so user-defined table/column names never flow through EF's model.
/// </summary>
public class PlatformDbContext : DbContext
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options)
    {
    }

    public DbSet<AppDefinition> Applications => Set<AppDefinition>();
    public DbSet<ApplicationVersion> ApplicationVersions => Set<ApplicationVersion>();
    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<UiComponent> Components => Set<UiComponent>();
    public DbSet<DataTable> Tables => Set<DataTable>();
    public DbSet<DataColumn> Columns => Set<DataColumn>();
    public DbSet<ApiEndpoint> Apis => Set<ApiEndpoint>();
    public DbSet<ServiceDefinition> Services => Set<ServiceDefinition>();
    public DbSet<FieldMapping> Mappings => Set<FieldMapping>();
    public DbSet<LineageExecution> LineageExecutions => Set<LineageExecution>();
    public DbSet<LineageEvent> LineageEvents => Set<LineageEvent>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("platform");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
    }
}
