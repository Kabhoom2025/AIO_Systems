using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class PosSaleConfiguration : IEntityTypeConfiguration<PosSale>
{
    public void Configure(EntityTypeBuilder<PosSale> builder)
    {
        builder.ToTable("PosSales");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(s => new { s.OrganizationId, s.SaleNumber }).IsUnique();

        builder.HasOne(s => s.Organization)
               .WithMany()
               .HasForeignKey(s => s.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Warehouse)
               .WithMany()
               .HasForeignKey(s => s.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CustomerAccount)
               .WithMany()
               .HasForeignKey(s => s.CustomerAccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.RevenueLedgerAccount)
               .WithMany()
               .HasForeignKey(s => s.RevenueLedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PaymentLedgerAccount)
               .WithMany()
               .HasForeignKey(s => s.PaymentLedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Owner)
               .WithMany()
               .HasForeignKey(s => s.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PostedJournalEntry)
               .WithMany()
               .HasForeignKey(s => s.PostedJournalEntryId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PosSaleLineConfiguration : IEntityTypeConfiguration<PosSaleLine>
{
    public void Configure(EntityTypeBuilder<PosSaleLine> builder)
    {
        builder.ToTable("PosSaleLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.PosSale)
               .WithMany(s => s.Lines)
               .HasForeignKey(l => l.PosSaleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Product)
               .WithMany()
               .HasForeignKey(l => l.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
