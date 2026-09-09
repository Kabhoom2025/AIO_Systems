using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class ThirdPartyDeliveryConfigConfiguration : IEntityTypeConfiguration<ThirdPartyDeliveryConfig>
{
    public void Configure(EntityTypeBuilder<ThirdPartyDeliveryConfig> builder)
    {
        builder.ToTable("ThirdPartyDeliveryConfigs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Provider).IsRequired().HasMaxLength(100);
        builder.Property(c => c.ApiKey).IsRequired().HasMaxLength(500);
        builder.Property(c => c.ApiSecret).HasMaxLength(500);
        builder.Property(c => c.WebhookUrl).HasMaxLength(500);
        builder.HasOne(c => c.Organization).WithMany()
            .HasForeignKey(c => c.OrganizationId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(c => new { c.Provider, c.OrganizationId }).IsUnique();
    }
}
