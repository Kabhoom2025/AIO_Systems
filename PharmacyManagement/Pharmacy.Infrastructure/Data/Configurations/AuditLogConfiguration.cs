using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.UserName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.HttpMethod).IsRequired().HasMaxLength(10);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(20);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);

        builder.HasIndex(a => new { a.OrganizationId, a.Timestamp });
    }
}
