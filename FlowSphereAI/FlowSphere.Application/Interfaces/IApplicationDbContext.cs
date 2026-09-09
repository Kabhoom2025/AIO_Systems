using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Interfaces;

/// <summary>
/// Application-layer abstraction over FlowSphereDbContext so Application handlers never take
/// a direct dependency on Infrastructure/EF Core internals.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowVersion> WorkflowVersions { get; }
    DbSet<WorkflowExecution> WorkflowExecutions { get; }
    DbSet<WorkflowStageDeployment> WorkflowStageDeployments { get; }
    DbSet<ExecutionStepLog> ExecutionStepLogs { get; }
    DbSet<Connector> Connectors { get; }
    DbSet<ConnectorCredential> ConnectorCredentials { get; }
    DbSet<Workspace> Workspaces { get; }
    DbSet<AppDefinition> AppDefinitions { get; }
    DbSet<TableDefinition> TableDefinitions { get; }
    DbSet<AppRecord> AppRecords { get; }
    DbSet<TableRecord> TableRecords { get; }
    DbSet<DeploymentLogEntry> DeploymentLogEntries { get; }
    DbSet<AppPublishHistoryEntry> AppPublishHistoryEntries { get; }
    DbSet<WorkspaceMembership> WorkspaceMemberships { get; }
    DbSet<WorkspaceRole> WorkspaceRoles { get; }
    DbSet<WorkspaceRoleAssignment> WorkspaceRoleAssignments { get; }
    DbSet<AppNotificationRule> AppNotificationRules { get; }
    DbSet<AppRecordComment> AppRecordComments { get; }
    DbSet<AppOtpChallenge> AppOtpChallenges { get; }
    DbSet<AppTrigger> AppTriggers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
