using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class DeploymentLogEntryConfiguration : IEntityTypeConfiguration<DeploymentLogEntry>
{
    public void Configure(EntityTypeBuilder<DeploymentLogEntry> builder)
    {
        builder.ToTable("deployment_log_entries");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.AppName).IsRequired().HasMaxLength(200);

        builder.HasIndex(d => new { d.OrganizationId, d.SourceGroupId });

        builder.HasOne(d => d.PerformedByUser)
            .WithMany()
            .HasForeignKey(d => d.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
