using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(30);
        builder.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Amount).HasColumnType("numeric(12,2)");
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasOne(e => e.Organization)
               .WithMany(o => o.Expenses)
               .HasForeignKey(e => e.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Branch)
               .WithMany()
               .HasForeignKey(e => e.BranchId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
