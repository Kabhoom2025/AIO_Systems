using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class PointsTransactionConfiguration : IEntityTypeConfiguration<PointsTransaction>
{
    public void Configure(EntityTypeBuilder<PointsTransaction> builder)
    {
        builder.ToTable("PointsTransactions");
        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Type).IsRequired().HasMaxLength(20);
        builder.Property(pt => pt.Description).IsRequired().HasMaxLength(200);
        builder.Property(pt => pt.CreatedDate).IsRequired();

        builder.HasOne(pt => pt.Order)
               .WithMany(o => o.PointsTransactions)
               .HasForeignKey(pt => pt.OrderId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(pt => pt.CustomerId);
        builder.HasIndex(pt => pt.OrderId);
    }
}
