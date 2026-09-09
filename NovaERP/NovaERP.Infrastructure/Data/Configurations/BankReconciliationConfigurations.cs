using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class BankReconciliationConfiguration : IEntityTypeConfiguration<BankReconciliation>
{
    public void Configure(EntityTypeBuilder<BankReconciliation> builder)
    {
        builder.ToTable("BankReconciliations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Status).IsRequired().HasMaxLength(20);
        builder.Property(r => r.StatementEndingBalance).HasColumnType("decimal(18,2)");

        builder.HasOne(r => r.Organization)
               .WithMany()
               .HasForeignKey(r => r.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.LedgerAccount)
               .WithMany()
               .HasForeignKey(r => r.LedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Owner)
               .WithMany()
               .HasForeignKey(r => r.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BankStatementLineConfiguration : IEntityTypeConfiguration<BankStatementLine>
{
    public void Configure(EntityTypeBuilder<BankStatementLine> builder)
    {
        builder.ToTable("BankStatementLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Amount).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.BankReconciliation)
               .WithMany(r => r.Lines)
               .HasForeignKey(l => l.BankReconciliationId)
               .OnDelete(DeleteBehavior.Cascade);

        // SetNull — matching an existing JournalEntryLine doesn't own it; unmatching or
        // deleting the journal entry (which never happens for Posted entries anyway) shouldn't
        // cascade-delete the statement line itself.
        builder.HasOne(l => l.MatchedJournalEntryLine)
               .WithMany()
               .HasForeignKey(l => l.MatchedJournalEntryLineId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
