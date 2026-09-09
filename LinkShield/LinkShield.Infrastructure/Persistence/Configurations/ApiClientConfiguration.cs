using LinkShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkShield.Infrastructure.Persistence.Configurations;

public class ApiClientConfiguration : IEntityTypeConfiguration<ApiClient>
{
    public void Configure(EntityTypeBuilder<ApiClient> builder)
    {
        builder.Property(c => c.OrganizationName).HasMaxLength(200).IsRequired();
    }
}

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasIndex(k => k.KeyHash).IsUnique();
        builder.Property(k => k.KeyHash).HasMaxLength(256).IsRequired();
        builder.Property(k => k.KeyPrefix).HasMaxLength(16).IsRequired();

        builder.HasOne(k => k.ApiClient)
            .WithMany(c => c.ApiKeys)
            .HasForeignKey(k => k.ApiClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
