using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Interfaces;

/// <summary>
/// DbContext abstraction that every MediatR handler depends on instead of the concrete
/// ProjectFlowDbContext, so handlers can be unit-tested against an EF Core InMemory-backed
/// implementation (or a hand-rolled fake) without pulling in Npgsql.
/// </summary>
public interface IProjectFlowDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<Department> Departments { get; }
    DbSet<Team> Teams { get; }
    DbSet<TeamMember> TeamMembers { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Invitation> Invitations { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<Milestone> Milestones { get; }
    DbSet<Label> Labels { get; }
    DbSet<WorkItem> WorkItems { get; }
    DbSet<ChecklistItem> ChecklistItems { get; }
    DbSet<WorkItemLabel> WorkItemLabels { get; }
    DbSet<WorkItemFollower> WorkItemFollowers { get; }
    DbSet<WorkItemDependency> WorkItemDependencies { get; }
    DbSet<WorkItemComment> WorkItemComments { get; }
    DbSet<WorkItemCommentMention> WorkItemCommentMentions { get; }
    DbSet<WorkItemAttachment> WorkItemAttachments { get; }
    DbSet<WorkItemActivity> WorkItemActivities { get; }
    DbSet<WorkItemTimeLog> WorkItemTimeLogs { get; }
    DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; }
    DbSet<CustomFieldValue> CustomFieldValues { get; }

    DbSet<Sprint> Sprints { get; }
    DbSet<RetrospectiveNote> RetrospectiveNotes { get; }
    DbSet<ActiveTimer> ActiveTimers { get; }
    DbSet<GanttBaseline> GanttBaselines { get; }
    DbSet<GanttBaselineItem> GanttBaselineItems { get; }

    // Phase 4: Chat / Notifications / Documents / Wiki
    DbSet<ChatChannel> ChatChannels { get; }
    DbSet<ChatChannelMember> ChatChannelMembers { get; }
    DbSet<ChannelRead> ChannelReads { get; }
    DbSet<DirectConversation> DirectConversations { get; }
    DbSet<ConversationRead> ConversationReads { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<MessageReaction> MessageReactions { get; }
    DbSet<MessageAttachment> MessageAttachments { get; }
    DbSet<UserPresence> UserPresences { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }
    DbSet<OrganizationIntegrationSettings> OrganizationIntegrationSettings { get; }
    DbSet<DocPage> DocPages { get; }
    DbSet<DocPageVersion> DocPageVersions { get; }
    DbSet<DocPageComment> DocPageComments { get; }

    // Phase 6: AI Features / Workflow Automation
    DbSet<AiChatConversation> AiChatConversations { get; }
    DbSet<AiChatMessage> AiChatMessages { get; }
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowCondition> WorkflowConditions { get; }
    DbSet<WorkflowAction> WorkflowActions { get; }
    DbSet<WorkflowRun> WorkflowRuns { get; }
    DbSet<WorkflowApprovalRequest> WorkflowApprovalRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
