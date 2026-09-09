using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.EntityType).IsRequired().HasMaxLength(100);
        builder.HasIndex(w => new { w.OrganizationId, w.EntityType });

        builder.HasOne(w => w.Organization)
               .WithMany()
               .HasForeignKey(w => w.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        // Steps are owned by their definition — cascade delete keeps the "replace all steps on
        // update" service logic simple and avoids orphaned step rows.
        builder.HasMany(w => w.Steps)
               .WithOne(s => s.WorkflowDefinition)
               .HasForeignKey(s => s.WorkflowDefinitionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowStepDefinitionConfiguration : IEntityTypeConfiguration<WorkflowStepDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowStepDefinition> builder)
    {
        builder.ToTable("WorkflowStepDefinitions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.MinAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(s => new { s.WorkflowDefinitionId, s.StepOrder });

        builder.HasOne(s => s.ApproverRole)
               .WithMany()
               .HasForeignKey(s => s.ApproverRoleId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(w => w.Status).IsRequired().HasMaxLength(20);
        builder.Property(w => w.Amount).HasColumnType("decimal(18,2)");
        builder.HasIndex(w => new { w.EntityType, w.EntityId });

        // Restrict: a WorkflowDefinition with live instances must not be hard-deleted (see
        // WorkflowDefinitionService.DeleteAsync, which checks HasInstancesAsync first and
        // throws a friendly error instead of ever attempting the delete).
        builder.HasOne(w => w.WorkflowDefinition)
               .WithMany()
               .HasForeignKey(w => w.WorkflowDefinitionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.SubmittedByUser)
               .WithMany()
               .HasForeignKey(w => w.SubmittedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Steps)
               .WithOne(s => s.WorkflowInstance)
               .HasForeignKey(s => s.WorkflowInstanceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowStepInstanceConfiguration : IEntityTypeConfiguration<WorkflowStepInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowStepInstance> builder)
    {
        builder.ToTable("WorkflowStepInstances");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Comments).HasMaxLength(1000);
        builder.HasIndex(s => new { s.WorkflowInstanceId, s.StepOrder });

        builder.HasOne(s => s.ApproverRole)
               .WithMany()
               .HasForeignKey(s => s.ApproverRoleId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.ApproverUser)
               .WithMany()
               .HasForeignKey(s => s.ApproverUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("AutomationRules");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.TriggerEvent).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ActionType).IsRequired().HasMaxLength(50);
        builder.Property(a => a.NotifyMessageTemplate).IsRequired().HasMaxLength(500);
        builder.HasIndex(a => new { a.OrganizationId, a.TriggerEvent, a.IsEnabled });

        builder.HasOne(a => a.Organization)
               .WithMany()
               .HasForeignKey(a => a.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.NotifyRole)
               .WithMany()
               .HasForeignKey(a => a.NotifyRoleId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
