using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(v => new { v.OrganizationId, v.Name });

        builder.HasOne(v => v.Organization)
               .WithMany()
               .HasForeignKey(v => v.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Owner)
               .WithMany()
               .HasForeignKey(v => v.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.User)
               .WithMany()
               .HasForeignKey(v => v.UserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class RfqRequestConfiguration : IEntityTypeConfiguration<RfqRequest>
{
    public void Configure(EntityTypeBuilder<RfqRequest> builder)
    {
        builder.ToTable("RfqRequests");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RfqNumber).IsRequired(false).HasMaxLength(20);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(r => new { r.OrganizationId, r.RfqNumber }).IsUnique();
        builder.HasIndex(r => new { r.OrganizationId, r.Status });

        builder.HasOne(r => r.Organization)
               .WithMany()
               .HasForeignKey(r => r.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Owner)
               .WithMany()
               .HasForeignKey(r => r.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.WinningVendor)
               .WithMany()
               .HasForeignKey(r => r.WinningVendorId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class RfqItemConfiguration : IEntityTypeConfiguration<RfqItem>
{
    public void Configure(EntityTypeBuilder<RfqItem> builder)
    {
        builder.ToTable("RfqItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ItemName).IsRequired().HasMaxLength(150);
        builder.Property(i => i.Quantity).HasColumnType("decimal(18,4)");

        builder.HasOne(i => i.RfqRequest)
               .WithMany(r => r.Items)
               .HasForeignKey(i => i.RfqRequestId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RfqVendorQuoteConfiguration : IEntityTypeConfiguration<RfqVendorQuote>
{
    public void Configure(EntityTypeBuilder<RfqVendorQuote> builder)
    {
        builder.ToTable("RfqVendorQuotes");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.QuotedAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(q => new { q.RfqRequestId, q.VendorId }).IsUnique();

        builder.HasOne(q => q.RfqRequest)
               .WithMany(r => r.Quotes)
               .HasForeignKey(q => q.RfqRequestId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.Vendor)
               .WithMany()
               .HasForeignKey(q => q.VendorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
