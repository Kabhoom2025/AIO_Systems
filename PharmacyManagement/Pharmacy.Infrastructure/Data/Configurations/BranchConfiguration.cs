using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(30);
        builder.Property(b => b.Type).IsRequired().HasMaxLength(20);

        builder.HasOne(b => b.Organization)
               .WithMany(o => o.Branches)
               .HasForeignKey(b => b.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
