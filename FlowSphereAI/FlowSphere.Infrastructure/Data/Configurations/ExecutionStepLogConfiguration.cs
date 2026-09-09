using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class ExecutionStepLogConfiguration : IEntityTypeConfiguration<ExecutionStepLog>
{
    public void Configure(EntityTypeBuilder<ExecutionStepLog> builder)
    {
        builder.ToTable("execution_step_logs");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.NodeType).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.NodeKey).IsRequired().HasMaxLength(100);

        builder.HasIndex(s => new { s.WorkflowExecutionId, s.StartedAt });

        builder.HasOne(s => s.WorkflowExecution)
            .WithMany(e => e.StepLogs)
            .HasForeignKey(s => s.WorkflowExecutionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
