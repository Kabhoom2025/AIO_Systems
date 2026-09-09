using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Infrastructure.Data.Configurations;

public class DocPageConfiguration : IEntityTypeConfiguration<DocPage>
{
    public void Configure(EntityTypeBuilder<DocPage> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Scope).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Content).IsRequired();
        builder.HasIndex(p => new { p.OrganizationId, p.ProjectId, p.Scope });

        builder.HasOne(p => p.Organization).WithMany().HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Project).WithMany().HasForeignKey(p => p.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.CreatedByUser).WithMany().HasForeignKey(p => p.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.UpdatedByUser).WithMany().HasForeignKey(p => p.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);

        // Self-referencing page tree -> Restrict, mirrors WorkItem's ParentWorkItem pattern.
        builder.HasOne(p => p.ParentPage).WithMany(p => p.ChildPages).HasForeignKey(p => p.ParentPageId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DocPageVersionConfiguration : IEntityTypeConfiguration<DocPageVersion>
{
    public void Configure(EntityTypeBuilder<DocPageVersion> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Content).IsRequired();
        builder.HasIndex(v => new { v.PageId, v.VersionNumber }).IsUnique();

        builder.HasOne(v => v.Page).WithMany(p => p.Versions).HasForeignKey(v => v.PageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(v => v.EditedByUser).WithMany().HasForeignKey(v => v.EditedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DocPageCommentConfiguration : IEntityTypeConfiguration<DocPageComment>
{
    public void Configure(EntityTypeBuilder<DocPageComment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Body).IsRequired();

        builder.HasOne(c => c.Page).WithMany(p => p.Comments).HasForeignKey(c => c.PageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.AuthorUser).WithMany().HasForeignKey(c => c.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
