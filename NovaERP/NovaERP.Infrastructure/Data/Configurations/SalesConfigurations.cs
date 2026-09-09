using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).IsRequired(false).HasMaxLength(20);
        builder.Property(o => o.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(o => new { o.OrganizationId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => new { o.OrganizationId, o.Status });

        builder.HasOne(o => o.Organization)
               .WithMany()
               .HasForeignKey(o => o.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Account)
               .WithMany()
               .HasForeignKey(o => o.AccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Opportunity)
               .WithMany()
               .HasForeignKey(o => o.OpportunityId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Owner)
               .WithMany()
               .HasForeignKey(o => o.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ItemName).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TaxRatePercent).HasColumnType("decimal(9,4)");

        builder.HasOne(l => l.SalesOrder)
               .WithMany(o => o.Lines)
               .HasForeignKey(l => l.SalesOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.TaxCode)
               .WithMany()
               .HasForeignKey(l => l.TaxCodeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.Product)
               .WithMany()
               .HasForeignKey(l => l.ProductId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
