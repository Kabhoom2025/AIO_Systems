using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

public class LineageExecutionConfiguration : IEntityTypeConfiguration<LineageExecution>
{
    public void Configure(EntityTypeBuilder<LineageExecution> builder)
    {
        builder.ToTable("lineage_executions", "platform");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CorrelationId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => e.CorrelationId);
        builder.HasIndex(e => e.ApplicationId);
        builder.HasIndex(e => e.StartedAt);

        builder.HasMany(e => e.Events).WithOne(ev => ev.Execution!)
            .HasForeignKey(ev => ev.ExecutionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class LineageEventConfiguration : IEntityTypeConfiguration<LineageEvent>
{
    public void Configure(EntityTypeBuilder<LineageEvent> builder)
    {
        builder.ToTable("lineage_events", "platform");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.NodeId).HasMaxLength(200).IsRequired();
        builder.Property(e => e.NodeType).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.EventType).HasConversion<string>().HasMaxLength(40);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.MetadataJson).HasColumnType("jsonb");
        builder.Property(e => e.ErrorCode).HasMaxLength(100);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(e => e.ExecutionId);

        builder.HasOne<LineageEvent>().WithMany()
            .HasForeignKey(e => e.ParentEventId).OnDelete(DeleteBehavior.Restrict);
    }
}
