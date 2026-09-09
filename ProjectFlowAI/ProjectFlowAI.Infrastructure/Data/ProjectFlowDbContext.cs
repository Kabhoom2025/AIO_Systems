using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Infrastructure.Data;

public class ProjectFlowDbContext : DbContext, IProjectFlowDbContext
{
    public ProjectFlowDbContext(DbContextOptions<ProjectFlowDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<WorkItemLabel> WorkItemLabels => Set<WorkItemLabel>();
    public DbSet<WorkItemFollower> WorkItemFollowers => Set<WorkItemFollower>();
    public DbSet<WorkItemDependency> WorkItemDependencies => Set<WorkItemDependency>();
    public DbSet<WorkItemComment> WorkItemComments => Set<WorkItemComment>();
    public DbSet<WorkItemCommentMention> WorkItemCommentMentions => Set<WorkItemCommentMention>();
    public DbSet<WorkItemAttachment> WorkItemAttachments => Set<WorkItemAttachment>();
    public DbSet<WorkItemActivity> WorkItemActivities => Set<WorkItemActivity>();
    public DbSet<WorkItemTimeLog> WorkItemTimeLogs => Set<WorkItemTimeLog>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();

    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<RetrospectiveNote> RetrospectiveNotes => Set<RetrospectiveNote>();
    public DbSet<ActiveTimer> ActiveTimers => Set<ActiveTimer>();
    public DbSet<GanttBaseline> GanttBaselines => Set<GanttBaseline>();
    public DbSet<GanttBaselineItem> GanttBaselineItems => Set<GanttBaselineItem>();

    // Phase 4: Chat / Notifications / Documents / Wiki
    public DbSet<ChatChannel> ChatChannels => Set<ChatChannel>();
    public DbSet<ChatChannelMember> ChatChannelMembers => Set<ChatChannelMember>();
    public DbSet<ChannelRead> ChannelReads => Set<ChannelRead>();
    public DbSet<DirectConversation> DirectConversations => Set<DirectConversation>();
    public DbSet<ConversationRead> ConversationReads => Set<ConversationRead>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();
    public DbSet<UserPresence> UserPresences => Set<UserPresence>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<OrganizationIntegrationSettings> OrganizationIntegrationSettings => Set<OrganizationIntegrationSettings>();
    public DbSet<DocPage> DocPages => Set<DocPage>();
    public DbSet<DocPageVersion> DocPageVersions => Set<DocPageVersion>();
    public DbSet<DocPageComment> DocPageComments => Set<DocPageComment>();

    // Phase 6: AI Features / Workflow Automation
    public DbSet<AiChatConversation> AiChatConversations => Set<AiChatConversation>();
    public DbSet<AiChatMessage> AiChatMessages => Set<AiChatMessage>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowCondition> WorkflowConditions => Set<WorkflowCondition>();
    public DbSet<WorkflowAction> WorkflowActions => Set<WorkflowAction>();
    public DbSet<WorkflowRun> WorkflowRuns => Set<WorkflowRun>();
    public DbSet<WorkflowApprovalRequest> WorkflowApprovalRequests => Set<WorkflowApprovalRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    // Npgsql refuses to write a DateTime with Kind=Unspecified into "timestamp with time zone"
    // columns. Command/request DTOs commonly carry client-supplied date-only values (e.g. a
    // WorkItem DueDate typed as plain DateTime), which System.Text.Json deserializes with
    // Kind=Unspecified — this blew up at runtime on the very first WorkItem with a DueDate,
    // despite compiling and passing every earlier test that only exercised server-generated
    // (already-UTC) dates. Normalizing every DateTime/DateTime? to UTC at the model level, once,
    // closes off this whole bug class instead of patching it per-property as new date fields appear.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
    }

    private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() : base(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    private sealed class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public UtcNullableDateTimeConverter() : base(
            v => v.HasValue && v.Value.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        {
        }
    }
}
