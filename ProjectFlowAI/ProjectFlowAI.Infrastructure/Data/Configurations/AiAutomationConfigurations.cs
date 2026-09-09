using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Infrastructure.Data.Configurations;

public class AiChatConversationConfiguration : IEntityTypeConfiguration<AiChatConversation>
{
    public void Configure(EntityTypeBuilder<AiChatConversation> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(300);
        builder.HasIndex(c => new { c.UserId, c.ProjectId });

        builder.HasOne(c => c.Project).WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AiChatMessageConfiguration : IEntityTypeConfiguration<AiChatMessage>
{
    public void Configure(EntityTypeBuilder<AiChatMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.Content).IsRequired();
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt });

        builder.HasOne(m => m.Conversation).WithMany(c => c.Messages).HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(300);
        builder.Property(w => w.TriggerType).HasConversion<string>().HasMaxLength(32);
        builder.Property(w => w.TriggerConfigJson).IsRequired();
        builder.HasIndex(w => new { w.OrganizationId, w.ProjectId, w.TriggerType, w.IsEnabled });

        builder.HasOne(w => w.Organization).WithMany().HasForeignKey(w => w.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(w => w.Project).WithMany().HasForeignKey(w => w.ProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(w => w.CreatedByUser).WithMany().HasForeignKey(w => w.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Conditions).WithOne(c => c.WorkflowDefinition).HasForeignKey(c => c.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(w => w.Actions).WithOne(a => a.WorkflowDefinition).HasForeignKey(a => a.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(w => w.Runs).WithOne(r => r.WorkflowDefinition).HasForeignKey(r => r.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowConditionConfiguration : IEntityTypeConfiguration<WorkflowCondition>
{
    public void Configure(EntityTypeBuilder<WorkflowCondition> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FieldPath).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Operator).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.Value).IsRequired().HasMaxLength(500);
    }
}

public class WorkflowActionConfiguration : IEntityTypeConfiguration<WorkflowAction>
{
    public void Configure(EntityTypeBuilder<WorkflowAction> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ActionType).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.ActionConfigJson).IsRequired();
    }
}

public class WorkflowRunConfiguration : IEntityTypeConfiguration<WorkflowRun>
{
    public void Configure(EntityTypeBuilder<WorkflowRun> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.LogJson).IsRequired();
        builder.HasIndex(r => new { r.WorkflowDefinitionId, r.StartedAt });

        builder.HasMany(r => r.ApprovalRequests).WithOne(a => a.WorkflowRun).HasForeignKey(a => a.WorkflowRunId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowApprovalRequestConfiguration : IEntityTypeConfiguration<WorkflowApprovalRequest>
{
    public void Configure(EntityTypeBuilder<WorkflowApprovalRequest> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(a => new { a.RequestedApproverUserId, a.Status });
    }
}
