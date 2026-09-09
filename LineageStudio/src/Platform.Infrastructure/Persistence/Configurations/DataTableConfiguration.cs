using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

public class DataTableConfiguration : IEntityTypeConfiguration<DataTable>
{
    public void Configure(EntityTypeBuilder<DataTable> builder)
    {
        builder.ToTable("tables", "platform");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(63).IsRequired();
        builder.Property(t => t.SchemaName).HasMaxLength(63).IsRequired();
        builder.HasIndex(t => new { t.ApplicationId, t.Name }).IsUnique();

        builder.HasMany(t => t.Columns).WithOne(c => c.Table!)
            .HasForeignKey(c => c.TableId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DataColumnConfiguration : IEntityTypeConfiguration<DataColumn>
{
    public void Configure(EntityTypeBuilder<DataColumn> builder)
    {
        builder.ToTable("columns", "platform");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(63).IsRequired();
        builder.Property(c => c.DataType).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.DefaultValue).HasMaxLength(500);
        builder.HasIndex(c => new { c.TableId, c.Name }).IsUnique();

        builder.HasOne<DataTable>().WithMany()
            .HasForeignKey(c => c.ReferencesTableId).OnDelete(DeleteBehavior.Restrict);
    }
}
