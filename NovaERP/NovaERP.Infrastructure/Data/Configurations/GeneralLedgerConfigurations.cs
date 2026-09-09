using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class LedgerAccountConfiguration : IEntityTypeConfiguration<LedgerAccount>
{
    public void Configure(EntityTypeBuilder<LedgerAccount> builder)
    {
        builder.ToTable("LedgerAccounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Type).IsRequired().HasMaxLength(20);
        builder.HasIndex(a => new { a.OrganizationId, a.Code }).IsUnique();

        builder.HasOne(a => a.Organization)
               .WithMany()
               .HasForeignKey(a => a.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EntryNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => new { e.OrganizationId, e.EntryNumber }).IsUnique();

        builder.HasOne(e => e.Organization)
               .WithMany()
               .HasForeignKey(e => e.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Owner)
               .WithMany()
               .HasForeignKey(e => e.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Debit).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Credit).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.JournalEntry)
               .WithMany(e => e.Lines)
               .HasForeignKey(l => l.JournalEntryId)
               .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade — a LedgerAccount with posted history shouldn't be deletable
        // (the same "protect transactional history" reasoning as every other Restrict here).
        builder.HasOne(l => l.LedgerAccount)
               .WithMany(a => a.Lines)
               .HasForeignKey(l => l.LedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
