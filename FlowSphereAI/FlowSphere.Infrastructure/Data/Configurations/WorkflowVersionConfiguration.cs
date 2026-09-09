using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.ToTable("workflow_versions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(v => v.GraphJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.HasIndex(v => new { v.WorkflowDefinitionId, v.VersionNumber }).IsUnique();

        // Belt-and-suspenders alongside the domain invariant in WorkflowDefinition.Publish:
        // at most one Published version per definition, enforced at the DB level too.
        builder.HasIndex(v => v.WorkflowDefinitionId)
            .HasDatabaseName("IX_workflow_versions_single_published")
            .IsUnique()
            .HasFilter($"\"Status\" = '{nameof(VersionStatus.Published)}'");

        builder.HasOne(v => v.WorkflowDefinition)
            .WithMany(w => w.Versions)
            .HasForeignKey(v => v.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
