using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class VendorBillConfiguration : IEntityTypeConfiguration<VendorBill>
{
    public void Configure(EntityTypeBuilder<VendorBill> builder)
    {
        builder.ToTable("VendorBills");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BillNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(b => new { b.OrganizationId, b.BillNumber }).IsUnique();

        builder.HasOne(b => b.Organization)
               .WithMany()
               .HasForeignKey(b => b.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Vendor)
               .WithMany()
               .HasForeignKey(b => b.VendorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.PayableLedgerAccount)
               .WithMany()
               .HasForeignKey(b => b.PayableLedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Owner)
               .WithMany()
               .HasForeignKey(b => b.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        // SetNull — traceability links to GL entries, not ownership; a JournalEntry is never
        // deleted once posted anyway (no delete action exists for Posted entries).
        builder.HasOne(b => b.PostedJournalEntry)
               .WithMany()
               .HasForeignKey(b => b.PostedJournalEntryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.PaymentJournalEntry)
               .WithMany()
               .HasForeignKey(b => b.PaymentJournalEntryId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class VendorBillLineConfiguration : IEntityTypeConfiguration<VendorBillLine>
{
    public void Configure(EntityTypeBuilder<VendorBillLine> builder)
    {
        builder.ToTable("VendorBillLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Amount).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.VendorBill)
               .WithMany(b => b.Lines)
               .HasForeignKey(l => l.VendorBillId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.LedgerAccount)
               .WithMany()
               .HasForeignKey(l => l.LedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
