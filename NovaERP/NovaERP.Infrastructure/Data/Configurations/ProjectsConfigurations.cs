using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Status).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Budget).HasColumnType("decimal(18,2)");
        builder.HasIndex(p => new { p.OrganizationId, p.Code }).IsUnique();

        builder.HasOne(p => p.Organization)
               .WithMany()
               .HasForeignKey(p => p.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Manager)
               .WithMany()
               .HasForeignKey(p => p.ManagerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.ToTable("ProjectTasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Priority).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Status).IsRequired().HasMaxLength(20);

        builder.HasOne(t => t.Organization)
               .WithMany()
               .HasForeignKey(t => t.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        // Cascade, not Restrict — a project's tasks are genuinely owned by its lifecycle,
        // unlike financial lines where Restrict protects transactional history.
        builder.HasOne(t => t.Project)
               .WithMany(p => p.Tasks)
               .HasForeignKey(t => t.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.AssignedTo)
               .WithMany()
               .HasForeignKey(t => t.AssignedToId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
