using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Infrastructure.Data;

public class FlowSphereDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public FlowSphereDbContext(DbContextOptions<FlowSphereDbContext> options, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
        : base(options)
    {
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    /// <summary>Read by TenantModelCacheKeyFactory so EF Core compiles a distinct model per
    /// tenant instead of freezing the query filter to whichever tenant built the model first.</summary>
    public int CurrentOrganizationId => _currentUser.OrganizationId;

    /// <summary>Read by TenantModelCacheKeyFactory for the same reason as CurrentOrganizationId,
    /// but for the Stage half of the combined query filter.</summary>
    public FlowSphere.Domain.Enums.EnvironmentStage CurrentEnvironmentStage => _currentEnvironment.Stage;

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowVersion> WorkflowVersions => Set<WorkflowVersion>();
    public DbSet<WorkflowExecution> WorkflowExecutions => Set<WorkflowExecution>();
    public DbSet<WorkflowStageDeployment> WorkflowStageDeployments => Set<WorkflowStageDeployment>();
    public DbSet<ExecutionStepLog> ExecutionStepLogs => Set<ExecutionStepLog>();
    public DbSet<Connector> Connectors => Set<Connector>();
    public DbSet<ConnectorCredential> ConnectorCredentials => Set<ConnectorCredential>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<AppDefinition> AppDefinitions => Set<AppDefinition>();
    public DbSet<TableDefinition> TableDefinitions => Set<TableDefinition>();
    public DbSet<AppRecord> AppRecords => Set<AppRecord>();
    public DbSet<TableRecord> TableRecords => Set<TableRecord>();
    public DbSet<DeploymentLogEntry> DeploymentLogEntries => Set<DeploymentLogEntry>();
    public DbSet<AppPublishHistoryEntry> AppPublishHistoryEntries => Set<AppPublishHistoryEntry>();
    public DbSet<WorkspaceMembership> WorkspaceMemberships => Set<WorkspaceMembership>();
    public DbSet<WorkspaceRole> WorkspaceRoles => Set<WorkspaceRole>();
    public DbSet<WorkspaceRoleAssignment> WorkspaceRoleAssignments => Set<WorkspaceRoleAssignment>();
    public DbSet<AppNotificationRule> AppNotificationRules => Set<AppNotificationRule>();
    public DbSet<AppRecordComment> AppRecordComments => Set<AppRecordComment>();
    public DbSet<AppOtpChallenge> AppOtpChallenges => Set<AppOtpChallenge>();
    public DbSet<AppTrigger> AppTriggers => Set<AppTrigger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlowSphereDbContext).Assembly);
        modelBuilder.ApplyGlobalFilters(_currentUser, _currentEnvironment);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
