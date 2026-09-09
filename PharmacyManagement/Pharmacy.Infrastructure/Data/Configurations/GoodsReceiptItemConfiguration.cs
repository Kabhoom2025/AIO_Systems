using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class GoodsReceiptItemConfiguration : IEntityTypeConfiguration<GoodsReceiptItem>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptItem> builder)
    {
        builder.ToTable("GoodsReceiptItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.BatchNumber).IsRequired().HasMaxLength(100);
        builder.Property(i => i.PurchasePrice).HasColumnType("numeric(10,2)");

        builder.HasOne(i => i.GoodsReceipt)
               .WithMany(g => g.Items)
               .HasForeignKey(i => i.GoodsReceiptId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Medicine)
               .WithMany()
               .HasForeignKey(i => i.MedicineId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
