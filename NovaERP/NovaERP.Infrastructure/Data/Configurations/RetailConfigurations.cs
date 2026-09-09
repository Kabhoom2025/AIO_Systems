using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(s => new { s.OrganizationId, s.Code }).IsUnique();

        builder.HasOne(s => s.Organization)
               .WithMany()
               .HasForeignKey(s => s.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Branch)
               .WithMany()
               .HasForeignKey(s => s.BranchId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Warehouse)
               .WithMany()
               .HasForeignKey(s => s.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
