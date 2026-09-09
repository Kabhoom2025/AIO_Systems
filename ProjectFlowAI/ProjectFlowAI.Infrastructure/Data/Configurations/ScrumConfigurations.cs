using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Infrastructure.Data.Configurations;

public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(s => new { s.ProjectId, s.Status });

        builder.HasOne(s => s.Project)
            .WithMany(p => p.Sprints)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RetrospectiveNoteConfiguration : IEntityTypeConfiguration<RetrospectiveNote>
{
    public void Configure(EntityTypeBuilder<RetrospectiveNote> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Category).HasConversion<string>().HasMaxLength(32);
        builder.Property(n => n.Text).IsRequired();

        builder.HasOne(n => n.Sprint)
            .WithMany(s => s.RetrospectiveNotes)
            .HasForeignKey(n => n.SprintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.CreatedByUser)
            .WithMany()
            .HasForeignKey(n => n.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ActiveTimerConfiguration : IEntityTypeConfiguration<ActiveTimer>
{
    public void Configure(EntityTypeBuilder<ActiveTimer> builder)
    {
        builder.HasKey(t => t.Id);
        // One running timer per user, enforced at the DB level.
        builder.HasIndex(t => t.UserId).IsUnique();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.WorkItem)
            .WithMany()
            .HasForeignKey(t => t.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GanttBaselineConfiguration : IEntityTypeConfiguration<GanttBaseline>
{
    public void Configure(EntityTypeBuilder<GanttBaseline> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(b => b.Project)
            .WithMany(p => p.GanttBaselines)
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GanttBaselineItemConfiguration : IEntityTypeConfiguration<GanttBaselineItem>
{
    public void Configure(EntityTypeBuilder<GanttBaselineItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Title).IsRequired().HasMaxLength(300);

        builder.HasOne(i => i.Baseline)
            .WithMany(b => b.Items)
            .HasForeignKey(i => i.BaselineId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: deleting a WorkItem should not silently delete historical baseline snapshots
        // that reference it — this is a point-in-time record, not a live join.
        builder.HasOne(i => i.WorkItem)
            .WithMany()
            .HasForeignKey(i => i.WorkItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
