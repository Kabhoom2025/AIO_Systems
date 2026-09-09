using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class TableRecordConfiguration : IEntityTypeConfiguration<TableRecord>
{
    public void Configure(EntityTypeBuilder<TableRecord> builder)
    {
        builder.ToTable("table_records");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.DataJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.HasIndex(r => r.TableDefinitionId);

        builder.HasOne(r => r.TableDefinition)
            .WithMany()
            .HasForeignKey(r => r.TableDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
