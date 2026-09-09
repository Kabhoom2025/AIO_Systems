using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.ReceiptNumber).IsRequired().HasMaxLength(50);
        builder.Property(g => g.ReceivedBy).HasMaxLength(200);

        builder.HasOne(g => g.Organization)
               .WithMany(o => o.GoodsReceipts)
               .HasForeignKey(g => g.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.PurchaseOrder)
               .WithMany(p => p.GoodsReceipts)
               .HasForeignKey(g => g.PurchaseOrderId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
