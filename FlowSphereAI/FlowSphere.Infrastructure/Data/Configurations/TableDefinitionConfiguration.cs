using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class TableDefinitionConfiguration : IEntityTypeConfiguration<TableDefinition>
{
    public void Configure(EntityTypeBuilder<TableDefinition> builder)
    {
        builder.ToTable("table_definitions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(2000);

        builder.Property(t => t.SchemaJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.HasIndex(t => new { t.WorkspaceId, t.Name });

        builder.HasOne(t => t.Workspace)
            .WithMany(w => w.Tables)
            .HasForeignKey(t => t.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
