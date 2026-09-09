using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class WorkflowStageDeploymentConfiguration : IEntityTypeConfiguration<WorkflowStageDeployment>
{
    public void Configure(EntityTypeBuilder<WorkflowStageDeployment> builder)
    {
        builder.ToTable("workflow_stage_deployments");
        builder.HasKey(d => d.Id);

        builder.HasIndex(d => new { d.WorkflowDefinitionId, d.Stage }).IsUnique();

        builder.HasOne(d => d.WorkflowDefinition)
            .WithMany()
            .HasForeignKey(d => d.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.WorkflowVersion)
            .WithMany()
            .HasForeignKey(d => d.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
