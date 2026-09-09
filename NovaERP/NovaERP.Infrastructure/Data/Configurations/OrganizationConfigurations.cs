using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Code).IsRequired().HasMaxLength(30);
        builder.HasIndex(o => o.Code).IsUnique();
    }
}

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(30);
        builder.HasIndex(b => new { b.OrganizationId, b.Code }).IsUnique();

        builder.HasOne(b => b.Organization)
               .WithMany(o => o.Branches)
               .HasForeignKey(b => b.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(30);
        builder.HasIndex(d => new { d.OrganizationId, d.Code }).IsUnique();

        builder.HasOne(d => d.Organization)
               .WithMany(o => o.Departments)
               .HasForeignKey(d => d.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Branch)
               .WithMany()
               .HasForeignKey(d => d.BranchId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Parent)
               .WithMany(d => d.Children)
               .HasForeignKey(d => d.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
