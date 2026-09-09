using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class DeliveryChargeSlabConfiguration : IEntityTypeConfiguration<DeliveryChargeSlab>
{
    public void Configure(EntityTypeBuilder<DeliveryChargeSlab> builder)
    {
        builder.ToTable("DeliveryChargeSlabs");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.FromKm).HasColumnType("numeric(8,2)").IsRequired();
        builder.Property(s => s.ToKm).HasColumnType("numeric(8,2)").IsRequired();
        builder.Property(s => s.Charge).HasColumnType("numeric(10,2)").IsRequired();
        builder.HasOne(s => s.Organization).WithMany()
            .HasForeignKey(s => s.OrganizationId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(s => s.OrganizationId);
    }
}
