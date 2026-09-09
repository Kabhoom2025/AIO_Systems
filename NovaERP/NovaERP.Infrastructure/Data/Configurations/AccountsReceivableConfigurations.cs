using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class CustomerInvoiceConfiguration : IEntityTypeConfiguration<CustomerInvoice>
{
    public void Configure(EntityTypeBuilder<CustomerInvoice> builder)
    {
        builder.ToTable("CustomerInvoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(i => new { i.OrganizationId, i.InvoiceNumber }).IsUnique();

        builder.HasOne(i => i.Organization)
               .WithMany()
               .HasForeignKey(i => i.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Account)
               .WithMany()
               .HasForeignKey(i => i.AccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ReceivableLedgerAccount)
               .WithMany()
               .HasForeignKey(i => i.ReceivableLedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Owner)
               .WithMany()
               .HasForeignKey(i => i.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.PostedJournalEntry)
               .WithMany()
               .HasForeignKey(i => i.PostedJournalEntryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.PaymentJournalEntry)
               .WithMany()
               .HasForeignKey(i => i.PaymentJournalEntryId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CustomerInvoiceLineConfiguration : IEntityTypeConfiguration<CustomerInvoiceLine>
{
    public void Configure(EntityTypeBuilder<CustomerInvoiceLine> builder)
    {
        builder.ToTable("CustomerInvoiceLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Amount).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.CustomerInvoice)
               .WithMany(i => i.Lines)
               .HasForeignKey(l => l.CustomerInvoiceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.LedgerAccount)
               .WithMany()
               .HasForeignKey(l => l.LedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
