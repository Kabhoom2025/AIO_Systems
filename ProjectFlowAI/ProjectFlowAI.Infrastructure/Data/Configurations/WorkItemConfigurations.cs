using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Infrastructure.Data.Configurations;

public class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Title).IsRequired().HasMaxLength(300);
        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(w => w.Priority).HasConversion<string>().HasMaxLength(32);
        builder.Property(w => w.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(w => w.EstimatedHours).HasColumnType("numeric(9,2)");
        builder.HasIndex(w => new { w.ProjectId, w.Status });

        builder.HasOne(w => w.Project)
            .WithMany(p => p.WorkItems)
            .HasForeignKey(w => w.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing parent -> Restrict, mirrors Department's self-referencing pattern.
        builder.HasOne(w => w.ParentWorkItem)
            .WithMany()
            .HasForeignKey(w => w.ParentWorkItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.AssigneeUser)
            .WithMany()
            .HasForeignKey(w => w.AssigneeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.ReporterUser)
            .WithMany()
            .HasForeignKey(w => w.ReporterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Nullable FK -> Backlog items (SprintId == null) never join a Sprint row.
        builder.HasOne(w => w.Sprint)
            .WithMany(s => s.WorkItems)
            .HasForeignKey(w => w.SprintId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Text).IsRequired().HasMaxLength(500);

        builder.HasOne(c => c.WorkItem)
            .WithMany(w => w.ChecklistItems)
            .HasForeignKey(c => c.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkItemLabelConfiguration : IEntityTypeConfiguration<WorkItemLabel>
{
    public void Configure(EntityTypeBuilder<WorkItemLabel> builder)
    {
        builder.HasKey(wl => new { wl.WorkItemId, wl.LabelId });

        builder.HasOne(wl => wl.WorkItem)
            .WithMany(w => w.WorkItemLabels)
            .HasForeignKey(wl => wl.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wl => wl.Label)
            .WithMany(l => l.WorkItemLabels)
            .HasForeignKey(wl => wl.LabelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkItemFollowerConfiguration : IEntityTypeConfiguration<WorkItemFollower>
{
    public void Configure(EntityTypeBuilder<WorkItemFollower> builder)
    {
        builder.HasKey(f => new { f.WorkItemId, f.UserId });

        builder.HasOne(f => f.WorkItem)
            .WithMany(w => w.Followers)
            .HasForeignKey(f => f.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkItemDependencyConfiguration : IEntityTypeConfiguration<WorkItemDependency>
{
    public void Configure(EntityTypeBuilder<WorkItemDependency> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DependencyType).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(d => new { d.WorkItemId, d.DependsOnWorkItemId }).IsUnique();

        builder.HasOne(d => d.WorkItem)
            .WithMany(w => w.Dependencies)
            .HasForeignKey(d => d.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing "depends on" side stays Restrict to avoid a self-referencing cascade cycle.
        builder.HasOne(d => d.DependsOnWorkItem)
            .WithMany()
            .HasForeignKey(d => d.DependsOnWorkItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkItemCommentConfiguration : IEntityTypeConfiguration<WorkItemComment>
{
    public void Configure(EntityTypeBuilder<WorkItemComment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Body).IsRequired();

        builder.HasOne(c => c.WorkItem)
            .WithMany(w => w.Comments)
            .HasForeignKey(c => c.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AuthorUser)
            .WithMany()
            .HasForeignKey(c => c.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkItemCommentMentionConfiguration : IEntityTypeConfiguration<WorkItemCommentMention>
{
    public void Configure(EntityTypeBuilder<WorkItemCommentMention> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.CommentId, m.MentionedUserId }).IsUnique();

        builder.HasOne(m => m.Comment)
            .WithMany(c => c.Mentions)
            .HasForeignKey(m => m.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.MentionedUser)
            .WithMany()
            .HasForeignKey(m => m.MentionedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkItemAttachmentConfiguration : IEntityTypeConfiguration<WorkItemAttachment>
{
    public void Configure(EntityTypeBuilder<WorkItemAttachment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FileName).IsRequired().HasMaxLength(300);
        builder.Property(a => a.FileUrl).IsRequired().HasMaxLength(1000);

        builder.HasOne(a => a.WorkItem)
            .WithMany(w => w.Attachments)
            .HasForeignKey(a => a.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.UploadedByUser)
            .WithMany()
            .HasForeignKey(a => a.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkItemActivityConfiguration : IEntityTypeConfiguration<WorkItemActivity>
{
    public void Configure(EntityTypeBuilder<WorkItemActivity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.HasIndex(a => new { a.WorkItemId, a.CreatedAt });

        builder.HasOne(a => a.WorkItem)
            .WithMany(w => w.Activities)
            .HasForeignKey(a => a.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkItemTimeLogConfiguration : IEntityTypeConfiguration<WorkItemTimeLog>
{
    public void Configure(EntityTypeBuilder<WorkItemTimeLog> builder)
    {
        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.WorkItem)
            .WithMany(w => w.TimeLogs)
            .HasForeignKey(t => t.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);
        builder.Property(d => d.FieldType).HasConversion<string>().HasMaxLength(32);

        builder.HasOne(d => d.Project)
            .WithMany(p => p.CustomFieldDefinitions)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomFieldValueConfiguration : IEntityTypeConfiguration<CustomFieldValue>
{
    public void Configure(EntityTypeBuilder<CustomFieldValue> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.ValueJson).IsRequired();
        builder.HasIndex(v => new { v.WorkItemId, v.CustomFieldDefinitionId }).IsUnique();

        builder.HasOne(v => v.WorkItem)
            .WithMany(w => w.CustomFieldValues)
            .HasForeignKey(v => v.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.CustomFieldDefinition)
            .WithMany(d => d.Values)
            .HasForeignKey(v => v.CustomFieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
