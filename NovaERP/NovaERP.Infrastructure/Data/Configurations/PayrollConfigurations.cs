using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class EmployeeCompensationConfiguration : IEntityTypeConfiguration<EmployeeCompensation>
{
    public void Configure(EntityTypeBuilder<EmployeeCompensation> builder)
    {
        builder.ToTable("EmployeeCompensations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Hra).HasColumnType("decimal(18,2)");
        builder.Property(c => c.OtherAllowances).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Deductions).HasColumnType("decimal(18,2)");
        builder.HasIndex(c => new { c.OrganizationId, c.EmployeeId }).IsUnique();

        builder.HasOne(c => c.Organization)
               .WithMany()
               .HasForeignKey(c => c.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Employee)
               .WithMany()
               .HasForeignKey(c => c.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayRunConfiguration : IEntityTypeConfiguration<PayRun>
{
    public void Configure(EntityTypeBuilder<PayRun> builder)
    {
        builder.ToTable("PayRuns");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.RunNumber).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(p => new { p.OrganizationId, p.RunNumber }).IsUnique();

        builder.HasOne(p => p.Organization)
               .WithMany()
               .HasForeignKey(p => p.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ExpenseLedgerAccount)
               .WithMany()
               .HasForeignKey(p => p.ExpenseLedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.DeductionsPayableLedgerAccount)
               .WithMany()
               .HasForeignKey(p => p.DeductionsPayableLedgerAccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Owner)
               .WithMany()
               .HasForeignKey(p => p.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        // SetNull — traceability link to the GL entry Pay posted, not ownership; a JournalEntry
        // is never deleted once posted anyway (no delete action exists for Posted entries).
        builder.HasOne(p => p.PostedJournalEntry)
               .WithMany()
               .HasForeignKey(p => p.PostedJournalEntryId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PayRunLineConfiguration : IEntityTypeConfiguration<PayRunLine>
{
    public void Configure(EntityTypeBuilder<PayRunLine> builder)
    {
        builder.ToTable("PayRunLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Hra).HasColumnType("decimal(18,2)");
        builder.Property(l => l.OtherAllowances).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Deductions).HasColumnType("decimal(18,2)");
        builder.Property(l => l.GrossPay).HasColumnType("decimal(18,2)");
        builder.Property(l => l.NetPay).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.PayRun)
               .WithMany(p => p.Lines)
               .HasForeignKey(l => l.PayRunId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Employee)
               .WithMany()
               .HasForeignKey(l => l.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
