using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("Tables");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TableNumber).IsRequired();
        builder.Property(t => t.Capacity).IsRequired();
        builder.Property(t => t.Hall).IsRequired().HasMaxLength(20).HasDefaultValue("AC");
        builder.Property(t => t.IsActive).HasDefaultValue(true);

        builder.Property(t => t.CreatedDate).HasDefaultValueSql("NOW()");

        builder.HasOne(t => t.Branch)
            .WithMany()
            .HasForeignKey(t => t.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Same table number can exist in different halls of the same branch, and the
        // same (hall, table number) pair can repeat across different branches.
        builder.HasIndex(t => new { t.BranchId, t.Hall, t.TableNumber }).IsUnique();
    }
}
