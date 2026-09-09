using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workflow.Domain.Entities;

namespace Workflow.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Role).IsRequired().HasMaxLength(50);
        builder.HasIndex(u => u.Email).IsUnique();
    }
}

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Description).HasMaxLength(1000);
        builder.Property(w => w.TriggerType).IsRequired().HasMaxLength(100);
        builder.Property(w => w.WebhookToken).IsRequired().HasMaxLength(64);
        builder.HasIndex(w => new { w.OrganizationId, w.TriggerType });
        builder.HasIndex(w => w.WebhookToken).IsUnique();

        builder.HasOne(w => w.PublishedVersion)
               .WithMany()
               .HasForeignKey(w => w.PublishedVersionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.ToTable("WorkflowVersions");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.GraphJson).IsRequired();
        builder.Property(v => v.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(v => new { v.WorkflowDefinitionId, v.VersionNumber }).IsUnique();

        builder.HasOne(v => v.WorkflowDefinition)
               .WithMany(w => w.Versions)
               .HasForeignKey(v => v.WorkflowDefinitionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.PublishedByUser)
               .WithMany()
               .HasForeignKey(v => v.PublishedByUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class WorkflowExecutionConfiguration : IEntityTypeConfiguration<WorkflowExecution>
{
    public void Configure(EntityTypeBuilder<WorkflowExecution> builder)
    {
        builder.ToTable("WorkflowExecutions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TriggerEntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.Property(e => e.PathJson).IsRequired();
        builder.HasIndex(e => new { e.WorkflowDefinitionId, e.CreatedDate });

        builder.HasOne(e => e.WorkflowDefinition)
               .WithMany()
               .HasForeignKey(e => e.WorkflowDefinitionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WorkflowVersion)
               .WithMany()
               .HasForeignKey(e => e.WorkflowVersionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
