using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class WorkspaceRoleAssignmentConfiguration : IEntityTypeConfiguration<WorkspaceRoleAssignment>
{
    public void Configure(EntityTypeBuilder<WorkspaceRoleAssignment> builder)
    {
        builder.ToTable("workspace_role_assignments");
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.WorkspaceId, a.UserId, a.WorkspaceRoleId }).IsUnique();

        builder.HasOne(a => a.Workspace)
            .WithMany()
            .HasForeignKey(a => a.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.WorkspaceRole)
            .WithMany()
            .HasForeignKey(a => a.WorkspaceRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
