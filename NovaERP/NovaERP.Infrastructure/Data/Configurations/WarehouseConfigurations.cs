using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(150);
        builder.Property(w => w.Code).IsRequired().HasMaxLength(50);
        builder.Property(w => w.ContactName).HasMaxLength(150);
        builder.Property(w => w.Email).HasMaxLength(200);
        builder.Property(w => w.Phone).HasMaxLength(50);
        builder.Property(w => w.AddressLine2).HasMaxLength(300);
        builder.Property(w => w.City).HasMaxLength(100);
        builder.Property(w => w.State).HasMaxLength(100);
        builder.Property(w => w.PostalCode).HasMaxLength(20);
        builder.Property(w => w.Country).HasMaxLength(100);
        builder.Property(w => w.TaxType).HasMaxLength(50);
        builder.Property(w => w.TaxCountry).HasMaxLength(100);
        builder.Property(w => w.TaxId).HasMaxLength(100);
        builder.HasIndex(w => new { w.OrganizationId, w.Code }).IsUnique();

        builder.HasOne(w => w.Organization)
               .WithMany()
               .HasForeignKey(w => w.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        // Restrict — a Branch with warehouses attached shouldn't be deletable (same
        // "protect transactional history" reasoning as Opportunity.AccountId).
        builder.HasOne(w => w.Branch)
               .WithMany(b => b.Warehouses)
               .HasForeignKey(w => w.BranchId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Quantity).HasColumnType("decimal(18,4)");
        builder.HasIndex(t => new { t.OrganizationId, t.ProductId });

        builder.HasOne(t => t.Organization)
               .WithMany()
               .HasForeignKey(t => t.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Product)
               .WithMany()
               .HasForeignKey(t => t.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        // Two FKs to the same Warehouse table need two explicit, distinctly-named
        // relationships — EF can't infer which is which from convention alone.
        builder.HasOne(t => t.FromWarehouse)
               .WithMany()
               .HasForeignKey(t => t.FromWarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToWarehouse)
               .WithMany()
               .HasForeignKey(t => t.ToWarehouseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
