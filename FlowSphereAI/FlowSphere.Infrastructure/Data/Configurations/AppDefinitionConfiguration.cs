using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class AppDefinitionConfiguration : IEntityTypeConfiguration<AppDefinition>
{
    public void Configure(EntityTypeBuilder<AppDefinition> builder)
    {
        builder.ToTable("app_definitions");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(2000);
        builder.Property(a => a.Icon).HasMaxLength(50);

        builder.Property(a => a.FormSchemaJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(a => a.FieldMappingJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(a => a.BusinessRulesJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(a => a.AccessPermissionsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.HasIndex(a => new { a.WorkspaceId, a.Name });
        builder.HasIndex(a => new { a.WorkspaceId, a.Stage });
        builder.HasIndex(a => new { a.SourceGroupId, a.Stage }).IsUnique();

        builder.HasOne(a => a.Workspace)
            .WithMany(w => w.Apps)
            .HasForeignKey(a => a.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
